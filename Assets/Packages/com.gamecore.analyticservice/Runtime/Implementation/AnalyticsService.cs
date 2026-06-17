using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GameCore.GeneralExtensions;
using GameCore.LoggerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;
using Object = UnityEngine.Object;

namespace GameCore.AnalyticService
{
    public class AnalyticsService : IAnalyticsService
    {
        public event Action<EventSendingInfo> OnEventSent;

        private readonly ILogger _logger;

        private readonly Dictionary<string, IAnalyticsParametersProvider> _parameterProviders = new();

        private readonly Dictionary<string, IAnalyticsProvider> _analyticsProviders = new();

        private readonly Dictionary<string, IRuntimeEventProfile> _eventProfiles = new();

        private readonly Dictionary<string, IRuntimeFunnelProfile> _funnelProfiles = new();

        private bool _isInitialized;

        private readonly AnalyticsSessionWatcher _analyticsSessionWatcher;

        private readonly IParametersFormatter _parametersFormatter;

        private readonly AnalyticServiceSettings _settings;

        private readonly IAppLifecycleProvider _appLifecycleProvider;

        private readonly ConcurrentQueue<BufferedEventData> _bufferedEvents = new();

        public IReadOnlyCollection<ProviderInfo> AvailableProviders =>
            _analyticsProviders.Values.Select(p => p.GetProviderInfo()).ToArray();

        public IReadOnlyCollection<IEventProfile> EventProfiles => _eventProfiles.Values;
        public IReadOnlyCollection<IFunnelProfile> FunnelProfiles => _funnelProfiles.Values;

        /// <summary>
        ///     Creates a new instance of the AnalyticsService, responsible for managing event and funnel tracking,
        ///     session analytics, and lifecycle integration.
        /// </summary>
        /// <param name="logger">Logger instance used for internal debug and error output.</param>
        /// <param name="container">
        ///     Optional container with analytics configuration, including event and funnel profiles.
        ///     If null, service initializes with default settings and no profiles.
        /// </param>
        /// <param name="appLifecycleProvider">
        ///     Optional lifecycle provider for application foreground/background state tracking.
        ///     If null, a default provider will be created internally.
        /// </param>
        /// <param name="formatter">
        ///     Optional custom parameters formatter. If null, a default empty formatter is used.
        /// </param>
        public AnalyticsService(
            ILogger logger,
            IEventsContainer container = null,
            IAppLifecycleProvider appLifecycleProvider = null,
            IParametersFormatter formatter = null
        )
        {
            _logger = logger;
            _appLifecycleProvider = appLifecycleProvider ?? CreateAppLifecycleProvider();
            _analyticsSessionWatcher = new AnalyticsSessionWatcher(logger, _appLifecycleProvider);
            _settings = container?.Settings ?? new AnalyticServiceSettings(false);
            _parametersFormatter = formatter ?? new EmptyParametersFormatter();

            AnalyticServiceConstants.FunnelParameter = _settings.FunnelEvenParameter;
            AnalyticServiceConstants.FunnelStepParameter = _settings.FunnelEventStepParameter;

            if (container == null)
                return;

            foreach (IEventProfile profile in container.EventProfiles)
            {
                RuntimeEventProfile runtimeProfile = new(_logger, profile);
                _eventProfiles.Add(runtimeProfile.EventId, runtimeProfile);
            }

            foreach (IFunnelProfile funnel in container.FunnelProfiles)
            {
                RuntimeFunnelProfile funnelProfile = new(_logger, funnel);
                _funnelProfiles.Add(funnelProfile.FunnelId, funnelProfile);
            }
        }

        public void ApplyFunnelCompletionReport(FunnelCompletionReport report)
        {
            if (!_funnelProfiles.TryGetValue(report.FunnelId, out IRuntimeFunnelProfile profile))
            {
                throw new AnalyticsServiceException($"Funnel with id: {report.FunnelId} not registered.");
            }

            profile.ApplyCompletedSteps(report.CompletedSteps);
        }

        public void ApplyFunnelCompletionReports(IReadOnlyCollection<FunnelCompletionReport> reports)
        {
            foreach (FunnelCompletionReport report in reports)
            {
                ApplyFunnelCompletionReport(report);
            }
        }

        public IReadOnlyCollection<FunnelCompletionReport> GetFunnelsReport()
        {
            return _funnelProfiles.Values.Select(x => x.CompletionReport).ToArray();
        }

        public EventSendingInfo SendEvent(
            string id,
            IDictionary<string, object> additionalValues = null,
            IParametersFormatter formatter = null
        )
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (!_eventProfiles.TryGetValue(id, out IRuntimeEventProfile profile))
            {
                throw new AnalyticsServiceException($"Event with id: {id} not registered.");
            }

            EventSendingInfo sendingInfo = new(id, profile.Providers);


            if (!_isInitialized)
            {
                AddToBuffer(id, profile, additionalValues, now, formatter);
                _logger.Warning("Analytics service is not initialized.");
                OnEventSent?.Invoke(sendingInfo);

                return sendingInfo;
            }

            if (!ThreadHelper.IsOnMainThread)
            {
                AddToBuffer(id, profile, additionalValues, now, formatter);
                _logger.Warning("Analytics service was called from non main thread.");
                OnEventSent?.Invoke(sendingInfo);

                return sendingInfo;
            }


            SendEventInternal(profile, additionalValues, formatter, now);
            OnEventSent?.Invoke(sendingInfo);

            return sendingInfo;
        }

        public void ClearFunnel()
        {
            foreach (IRuntimeFunnelProfile funnel in _funnelProfiles.Values)
            {
                funnel.Clear();
            }
        }

        public void ClearFunnel(string funnelId)
        {
            IRuntimeFunnelProfile funnel = _funnelProfiles.Values.FirstOrDefault(f => f.FunnelId == funnelId);
            funnel?.Clear();
        }

        public IAnalyticsService RegisterEventProfile(IEventProfile profile)
        {
            if (_isInitialized)
            {
                throw new AnalyticsServiceException("Can't register event profile after initialization.");
            }

            if (_eventProfiles.ContainsKey(profile.EventId))
            {
                throw new AnalyticsServiceException($"Event profile with id: {profile.EventId} already registered.");
            }

            RuntimeEventProfile runtimeProfile = new(_logger, profile);
            _eventProfiles.Add(runtimeProfile.EventId, runtimeProfile);

            return this;
        }

        public IAnalyticsService RegisterParameterProvider(IAnalyticsParametersProvider parametersProvider)
        {
            if (_isInitialized)
            {
                throw new AnalyticsServiceException("Can't register parameter provider after initialization.");
            }

            foreach (string id in parametersProvider.ParametersId)
            {
                if (_parameterProviders.ContainsKey(id))
                {
                    throw new AnalyticsServiceException($"Parameter with id: {id} already added.");
                }

                _parameterProviders.Add(id, parametersProvider);
            }

            return this;
        }

        public IAnalyticsService RegisterAnalyticsProvider(IAnalyticsProvider analyticsProvider)
        {
            if (_isInitialized)
            {
                throw new AnalyticsServiceException("Can't register analytics provider after initialization.");
            }

            _analyticsProviders.Add(analyticsProvider.Id, analyticsProvider);

            return this;
        }

        public IAnalyticsService RegisterFunnelProfile(IFunnelProfile profile)
        {
            if (_isInitialized)
            {
                throw new AnalyticsServiceException("Can't register funnel profile after initialization.");
            }

            RuntimeFunnelProfile funnelProfile = new(_logger, profile);
            _funnelProfiles.Add(funnelProfile.FunnelId, funnelProfile);

            return this;
        }

        public async UniTask Initialize(IProgressReceiver progressReceiver)
        {
            if (_isInitialized)
                return;

            _analyticsSessionWatcher.Initialize(progressReceiver).Forget();

            InitializeParameterProviders();
            ConstructAnalyticsProviders();
            await InitializeAnalyticsProviders(progressReceiver);
            _isInitialized = true;
            Run().Forget();
        }

        public void Dispose()
        {
            _analyticsSessionWatcher?.Dispose();

            foreach (IAnalyticsProvider provider in _analyticsProviders.Values)
            {
                provider.Dispose();
            }
        }

        public EventSendingInfo SendFunnelEvent(
            string eventId,
            string funnelId,
            string stepId,
            IDictionary<string, object> additionalValues = null,
            IParametersFormatter formatter = null
        )
        {
            if (!_funnelProfiles.TryGetValue(funnelId, out IRuntimeFunnelProfile profile))
            {
                throw new AnalyticsServiceException($"Funnel with id: {funnelId} not registered.");
            }

            if (!profile.StepsIndexPairs.TryGetValue(stepId, out string index))
            {
                throw new AnalyticsServiceException($"Step with id: {stepId} not registered in funnel {eventId}.");
            }

            Dictionary<string, object> parameters = new()
            {
                { AnalyticServiceConstants.FunnelParameter, funnelId },
                { AnalyticServiceConstants.FunnelIndexParameter, index },
                { AnalyticServiceConstants.FunnelStepParameter, stepId }
            };

            if (additionalValues != null)
            {
                foreach (KeyValuePair<string, object> additionalValue in additionalValues)
                {
                    parameters.Add(additionalValue.Key, additionalValue.Value);
                }
            }

            return SendEvent(eventId, parameters, formatter);
        }

        private void AddToBuffer(
            string id,
            IRuntimeEventProfile profile,
            IDictionary<string, object> parameters,
            long createdAtTimestamp,
            IParametersFormatter formatter
        )
        {
            BufferedEventData bufferedEvent = new()
            {
                EventId = id,
                Profile = profile,
                Parameters = parameters,
                Formatter = formatter,
                CreatedAtTimestamp = createdAtTimestamp
            };
            _bufferedEvents.Enqueue(bufferedEvent);
        }

        private void SendEventInternal(BufferedEventData bufferedEvent)
        {
            SendEventInternal(
                bufferedEvent.Profile,
                bufferedEvent.Parameters,
                bufferedEvent.Formatter,
                bufferedEvent.CreatedAtTimestamp
            );
        }

        private void SendEventInternal(
            IRuntimeEventProfile profile,
            IDictionary<string, object> parameters,
            IParametersFormatter formatter,
            long createdAtTimestamp
        )
        {
            IEvent @event = CreateEvent(profile, createdAtTimestamp, parameters);

            if (!CanSend(profile, @event))
            {
                _logger.Warning($"Event {@event} not sent.");

                return;
            }

            foreach (string attachedEventId in profile.AttachedEvents)
            {
                if (!_eventProfiles.TryGetValue(attachedEventId, out IRuntimeEventProfile attachedProfile))
                {
                    throw new AnalyticsServiceException($"Attached event with id: {attachedEventId} not registered.");
                }

                IEvent attachedEvent = CreateEvent(attachedProfile, createdAtTimestamp, parameters);
                @event.Merge(attachedEvent);
            }

            IParametersFormatter currentFormatter = formatter ?? _parametersFormatter;
            @event.ReformatParameters(currentFormatter);

            SendToProviders(@event);
        }

        private void ConstructAnalyticsProviders()
        {
            foreach (IAnalyticsProvider provider in _analyticsProviders.Values)
            {
                provider.Construct(_appLifecycleProvider, _analyticsSessionWatcher);
            }
        }

        private void SendToProviders(IEvent @event)
        {
            if (@event.Providers == null || @event.Providers.Count == 0)
            {
                _logger.Warning($"Event with id: {@event.Id} do not has assigned analytics provider. Event not sent.");

                return;
            }

            if (_analyticsProviders.Count == 0)
            {
                _logger.Warning($"There is no registered analytics provider Event with id: {@event.Id} not sent.");

                return;
            }

            foreach (string providerId in @event.Providers)
            {
                if (!_analyticsProviders.TryGetValue(providerId, out IAnalyticsProvider analyticsProvider))
                {
                    throw new AnalyticsServiceException($"Provider with id: {providerId} not registered.");
                }

                analyticsProvider.SendEvent(@event);
            }
        }

        private IEvent CreateEvent(IEventProfile profile, long sentAt, IDictionary<string, object> additionalValues = null)
        {
            Dictionary<string, object> parameters = PrepareParameters(profile, additionalValues);


            string cedid = Guid.NewGuid().ToString();

            Event @event = new(
                profile.EventId,
                profile.EventType,
                profile.Providers,
                parameters,
                profile.CustomAttributes,
                sentAt,
                _analyticsSessionWatcher.SessionId,
                NonceUtils.GenerateNonce(),
                cedid,
                Mathf.RoundToInt((float)_analyticsSessionWatcher.SessionDuration.TotalSeconds)
            );

            return @event;
        }

        private bool CanSend(IRuntimeEventProfile eventProfile, IEvent @event)
        {
            if (@event.EventType == EEventType.Generic)
                return eventProfile.CanSend(@event);

            if (!@event.Parameters.TryGetValue(
                    AnalyticServiceConstants.FunnelParameter,
                    out object funnelIdParameter
                ))
            {
                throw new AnalyticsServiceException("Can't send funnel event with funnel id null.");
            }

            if (!(funnelIdParameter is string funnelId))
            {
                throw new AnalyticsServiceException("Funnel event funnel id parameter is not string.");
            }

            if (!_funnelProfiles.TryGetValue(funnelId, out IRuntimeFunnelProfile funnelProfile))
            {
                throw new AnalyticsServiceException($"Funnel with id: {funnelId} not registered.");
            }

            bool canSend = eventProfile.CanSend(@event);
            bool funnelCanSend = funnelProfile.CanSend(@event);

            return canSend && funnelCanSend;
        }

        private Dictionary<string, object> PrepareParameters(
            IEventProfile profile,
            IDictionary<string, object> additionalValues
        )
        {
            Dictionary<string, object> values = new();

            foreach (string parameter in profile.Parameters)
            {
                if (values.ContainsKey(parameter))
                    continue;

                if (_parameterProviders.TryGetValue(parameter, out IAnalyticsParametersProvider provider))
                {
                    object value = provider.GetValue(parameter);
                    values.Add(parameter, value);
                }
                else
                {
                    if (additionalValues != null && additionalValues.TryGetValue(parameter, out object value))
                    {
                        values.Add(parameter, value);
                    }
                    else
                    {
                        ThrowParameterNotResolvedException(parameter, profile.EventId);
                        values.Add(parameter, "value provider not resolved");
                    }
                }
            }

            if (additionalValues != null)
            {
                foreach (KeyValuePair<string, object> additionalValue in additionalValues)
                {
                    if (string.IsNullOrEmpty(additionalValue.Key))
                    {
                        _logger.Warning("Skipping additional parameter with null or empty key");
                        continue;
                    }

                    if (!values.ContainsKey(additionalValue.Key))
                    {
                        values.Add(additionalValue.Key, additionalValue.Value);
                    }
                }
            }

            return values;
        }

        private void ThrowParameterNotResolvedException(string parameter, string eventId)
        {
            if (!_settings.SuppressExceptions)
            {
                throw new AnalyticsServiceException($"Can't resolve parameter: {parameter} for event: {eventId}");
            }

            _logger.Warning($"Can't resolve parameter: {parameter} for event: {eventId}.");
        }

        private void InitializeParameterProviders()
        {
            foreach (IAnalyticsParametersProvider parameterProvider in _parameterProviders.Values)
            {
                parameterProvider.Initialize();
            }
        }

        private async UniTask InitializeAnalyticsProviders(IProgressReceiver progressReceiver)
        {
            Initializer initializer = new();
            initializer.Add(progressReceiver);
            foreach (IAnalyticsProvider provider in _analyticsProviders.Values)
            {
                initializer.Add(provider);
            }

            await initializer.Initialize();
        }

        private IAppLifecycleProvider CreateAppLifecycleProvider()
        {
            AppLifecycleComponent appLifecycleProvider = Object.FindAnyObjectByType<AppLifecycleComponent>();

            if (appLifecycleProvider)
                return appLifecycleProvider;

            GameObject appLifecycleProviderGo = new("AppLifecycleProvider");
            appLifecycleProvider = appLifecycleProviderGo.AddComponent<AppLifecycleComponent>();
            Object.DontDestroyOnLoad(appLifecycleProviderGo);

            return appLifecycleProvider;
        }

        private async UniTask Run()
        {
            while (_isInitialized)
            {
                EnsureBufferSent();
                await UniTask.Yield();
            }
        }

        private void EnsureBufferSent()
        {
            if (_bufferedEvents.IsEmpty)
                return;

            while (_bufferedEvents.TryDequeue(out BufferedEventData bufferedEvent))
            {
                SendEventInternal(bufferedEvent);
            }
        }
    }

    internal class BufferedEventData
    {
        public string EventId { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public IParametersFormatter Formatter { get; set; }
        public long CreatedAtTimestamp { get; set; }

        public IRuntimeEventProfile Profile { get; set; }
    }
}
