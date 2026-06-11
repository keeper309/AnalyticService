using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public interface IRuntimeFunnelProfile : IFunnelProfile
    {
        FunnelCompletionReport CompletionReport { get; }
        IReadOnlyDictionary<string, string> StepsIndexPairs { get; }

        bool CanSend(IEvent @event);
        void Clear();

        void ApplyCompletedSteps(IReadOnlyCollection<string> steps);
    }
}