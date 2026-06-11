using System;
using System.Collections.Generic;
using GameCore.GeneralExtensions;
using GameCore.LoggerService;
using Cysharp.Threading.Tasks;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Lightweight analytics provider used for development and debugging.
    ///     Instead of sending events to a remote server, it logs them locally using <see cref="ILogger" />.
    ///     Useful for validating event structure and flow during development.
    /// </summary>
    [AnalyticsProviderId(ProviderId, false)]
    public class DebugLogAnalyticsProvider : IAnalyticsProvider
    {
        public event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsSuccess;
        public event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsFailed;

        public const string ProviderId = "DebugLog";

        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _customAttributes = new();

        public string Id => ProviderId;

        public IReadOnlyDictionary<string, string> CustomAttributes => _customAttributes;

        public DebugLogAnalyticsProvider(ILogger logger)
        {
            _logger = logger;
        }

        public UniTask Initialize(IProgressReceiver progressReceiver)
        {
            return UniTask.CompletedTask;
        }

        public void SendEvent(IEvent @event)
        {
            _logger.Message(@event.ToString());
            OnEventsSuccess?.Invoke(this, new List<IEvent> { @event });
        }

        public void Construct(IAppLifecycleProvider appLifecycleProvider, AnalyticsSessionWatcher analyticsSessionWatcher) { }

        public void Dispose() { }
    }
}
