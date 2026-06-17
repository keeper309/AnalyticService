using System.Collections.Generic;
using System.Linq;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    public class RuntimeFunnelProfile : IRuntimeFunnelProfile
    {
        private readonly ILogger _logger;
        private readonly IFunnelStepsStorage _stepsStorage;

        public string FunnelId { get; }

        public IReadOnlyCollection<StepIndexPair> FunnelSteps { get; }

        public FunnelCompletionReport CompletionReport =>
            new FunnelCompletionReport(FunnelId, _stepsStorage.CompletedSteps);

        public IReadOnlyDictionary<string, string> StepsIndexPairs { get; }

        public RuntimeFunnelProfile(ILogger logger, IFunnelProfile profile)
        {
            _logger = logger;
            FunnelId = profile.FunnelId;
            FunnelSteps = profile.FunnelSteps.ToArray();
            _stepsStorage = new FunnelStepsStorage(FunnelId);
            StepsIndexPairs = FunnelSteps.ToDictionary(x => x.step, x => x.index);
        }

        public bool CanSend(IEvent @event)
        {
            if (!@event.Parameters.TryGetValue(
                    AnalyticServiceConstants.FunnelStepParameter,
                    out object value
                ))
            {
                throw new AnalyticsServiceException(
                    $"Parameter {AnalyticServiceConstants.FunnelStepParameter}, not presented in funnel event: {@event}"
                );
            }

            if (!(value is string stepId))
            {
                throw new AnalyticsServiceException("Funnel event step parameter is not string.");
            }

            if (_stepsStorage.ContainsStep(stepId))
            {
                return false;
            }

            _stepsStorage.AddStep(stepId);

            return true;
        }

        public void Clear()
        {
            _stepsStorage.Clear();
        }

        public void ApplyCompletedSteps(IReadOnlyCollection<string> steps)
        {
            foreach (string step in steps)
            {
                if (!StepsIndexPairs.ContainsKey(step))
                    _logger.Warning($"Funnel with id: {FunnelId} not contains step: {step}");

                if (_stepsStorage.ContainsStep(step))
                    continue;

                _stepsStorage.AddStep(step);
            }
        }
    }
}
