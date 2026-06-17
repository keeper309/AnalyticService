using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.GeneralExtensions;
using GameCore.LoggerService;

namespace GameCore.AnalyticService
{
    public class RuntimeEventProfile : IRuntimeEventProfile
    {
        private readonly ILogger _logger;
        private readonly string[] _parameters;
        private readonly string[] _attachedEvents;
        private readonly Dictionary<string, string> _customAttributes;
        private readonly Dictionary<string, IFunnelStepsStorage> _funnelsStorage;

        private string[] _providers;
        public IReadOnlyCollection<string> Providers => _providers;
        public EEventType EventType { get; }
        public string EventId { get; }

        public IReadOnlyCollection<string> Parameters => _parameters;
        public IReadOnlyDictionary<string, string> CustomAttributes => _customAttributes;
        public IReadOnlyCollection<string> AttachedEvents => _attachedEvents;

        public RuntimeEventProfile(ILogger logger, IEventProfile profile)
        {
            _logger = logger;
            EventId = profile.EventId;
            EventType = profile.EventType;
            _providers = profile.Providers?.ToArray() ?? Array.Empty<string>();
            _attachedEvents = profile.AttachedEvents?.ToArray() ?? Array.Empty<string>();
            _parameters = profile.Parameters?.ToArray() ?? Array.Empty<string>();
            _customAttributes = profile.CustomAttributes?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
            _providers = profile.Providers?.ToArray() ?? Array.Empty<string>();
            _funnelsStorage = new Dictionary<string, IFunnelStepsStorage>();
        }

        public bool CanSend(IEvent @event)
        {
            if (EventId != @event.Id)
            {
                throw new AnalyticsServiceException($"Event profile id {EventId} does not match event id {@event.Id}");
            }

            return true;
        }

        public void OverrideProviders(string[] providerIds)
        {
            _providers = providerIds.ToArray();
        }

        private IFunnelStepsStorage GetFunnelStorage(string funnelId)
        {
            if (_funnelsStorage.TryGetValue(funnelId, out IFunnelStepsStorage storage))
                return storage;

            storage = new FunnelStepsStorage(funnelId);
            _funnelsStorage.Add(funnelId, storage);

            return storage;
        }
    }
}
