#if ANALYTICS_ADJUST_EXISTS

using System;
using System.Collections.Generic;
using AdjustSdk;
using GameCore.GeneralExtensions;
using Cysharp.Threading.Tasks;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Analytics provider that integrates with the Adjust SDK.
    ///     Responsible for forwarding events and funnel data to Adjust,
    ///     and handling success/failure callbacks.
    /// </summary>
    [AnalyticsProviderId(ProviderId)]
    public class AdjustProvider : IAnalyticsProvider
    {
        public event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsSuccess;
        public event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsFailed;

        public const string ProviderId = "Adjust";

        public const string AdjustTokenAttributeKey = "adjust-token";

        private const string SessionIdAttributeKey = "sid";

        private readonly ILogger _logger;

        private readonly AdjustConfig _adjustConfig;

        private IAppLifecycleProvider _appLifecycleProvider;

        private AnalyticsSessionWatcher _analyticsSessionWatcher;

        private readonly Dictionary<string, string> _customAttributes = new();

        private string _cachedAdid;

        public string Id => ProviderId;

        public IReadOnlyDictionary<string, string> CustomAttributes => _customAttributes;

        public string CachedAdid => _cachedAdid;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AdjustProvider" /> with the specified logger, app token and configuration.
        /// </summary>
        /// <param name="logger">Logger instance used for internal diagnostics and debugging.</param>
        /// <param name="appToken">Adjust App Token, used to identify the app in the Adjust platform.</param>
        /// <param name="adjustConfig">Adjust configuration settings specific to the current platform and environment.</param>
        public AdjustProvider(ILogger logger, string appToken, AdjustConfig adjustConfig)
        {
            _logger = logger;
            _adjustConfig = adjustConfig;
            _customAttributes.Add(AdjustTokenAttributeKey, appToken);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AdjustProvider" /> with only the Adjust configuration.
        /// </summary>
        /// <param name="adjustConfig">Adjust configuration settings specific to the current platform and environment.</param>
        public AdjustProvider(AdjustConfig adjustConfig)
        {
            _adjustConfig = adjustConfig;
        }

        public UniTask Initialize(IProgressReceiver progressReceiver)
        {
            return UniTask.CompletedTask;
        }

        public void SendEvent(IEvent @event)
        {
            if (!@event.CustomAttributes.TryGetValue(AdjustTokenAttributeKey, out string token))
            {
                throw new AnalyticsServiceException("Adjust token is missing.");
            }

            AdjustEvent adjustEvent = new(token);
            foreach (KeyValuePair<string, object> parameter in @event.Parameters)
            {
                adjustEvent.AddCallbackParameter(parameter.Key, ParametersConverter.ToString(parameter.Value));
            }

            AdjustProxy.TrackEvent(adjustEvent);
        }

        public void Construct(IAppLifecycleProvider appLifecycleProvider, AnalyticsSessionWatcher analyticsSessionWatcher)
        {
            _appLifecycleProvider = appLifecycleProvider;
            _analyticsSessionWatcher = analyticsSessionWatcher;

            UpdateAnalyticsSessionIdCallbackParameter(_analyticsSessionWatcher.SessionId);

            _analyticsSessionWatcher.OnSessionStart += UpdateAnalyticsSessionIdCallbackParameter;

            _adjustConfig.SessionSuccessDelegate = OnSessionSuccessHandler;
            _adjustConfig.SessionFailureDelegate = OnSessionFailureHandler;
            _adjustConfig.EventSuccessDelegate = OnEventSuccessHandler;
            _adjustConfig.EventFailureDelegate = OnEventFailedHandler;

            AdjustProxy.InitSdk(_adjustConfig);
        }

        public void Dispose()
        {
            _analyticsSessionWatcher?.Dispose();
        }

        private void UpdateAnalyticsSessionIdCallbackParameter(string sessionId)
        {
            AdjustProxy.AddGlobalCallbackParameter(SessionIdAttributeKey, sessionId);
        }

        private void OnSessionSuccessHandler(AdjustSessionSuccess sessionSuccess)
        {
            if (!string.IsNullOrEmpty(sessionSuccess.Adid))
            {
                _cachedAdid = sessionSuccess.Adid;
            }

            _logger?.Print($"Session Success. Adid: {sessionSuccess.Adid}");
        }

        private void OnSessionFailureHandler(AdjustSessionFailure sessionFailure)
        {
            _logger?.Print($"Session Failed. Message: {sessionFailure.Message}, WillRetry: {sessionFailure.WillRetry}");
        }

        private void OnEventFailedHandler(AdjustEventFailure eventFailure)
        {
            _logger?.Print($"Event Failed. Token: {eventFailure.EventToken}");
        }

        private void OnEventSuccessHandler(AdjustEventSuccess eventSuccess)
        {
            _logger?.Print($"Event Success. Token: {eventSuccess.EventToken}");
        }
    }
}
#endif