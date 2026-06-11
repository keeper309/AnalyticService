using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore.AnalyticService
{
    [Serializable]
    public class EventProfile : IEventProfile
    {
        [SerializeField] public string eventId;

        [SerializeField] public EventType eventType;

        [SerializeField] public AnalyticsProviderId[] providers;

        [SerializeField] public Parameter[] observableParameters;

        [SerializeField] public ParametersCategory[] categories;

        [SerializeField] public List<string> parameters;

        [SerializeField] public List<CustomAttribute> customAttributes;

        [SerializeField] public List<EventReference> attachedEvents;

        public IReadOnlyCollection<string> Providers => providers?.Select(p => p.ProviderId).ToArray() ?? Array.Empty<string>();
        public EventType EventType => eventType;
        public string EventId => eventId;

        public IReadOnlyCollection<string> Parameters => GetParameters();

        public IReadOnlyDictionary<string, string> CustomAttributes =>
            customAttributes.ToDictionary(x => x.key, x => x.value);

        public IReadOnlyCollection<string> AttachedEvents => attachedEvents.Select(r => r.EventId).ToArray();

        public EventProfile(
            string eventId,
            EventType eventType,
            AnalyticsProviderId[] providers = null,
            Parameter[] observableParameters = null,
            ParametersCategory[] categories = null,
            IEnumerable<string> parameters = null,
            IDictionary<string, string> customAttributes = null,
            IEnumerable<string> attachedEvents = null
        )
        {
            this.eventId = eventId;
            this.eventType = eventType;
            this.providers = providers ?? Array.Empty<AnalyticsProviderId>();
            this.observableParameters = observableParameters ?? Array.Empty<Parameter>();
            this.categories = categories ?? Array.Empty<ParametersCategory>();
            this.parameters = parameters?.ToList() ?? new List<string>();
            this.customAttributes =
                customAttributes?.Select(p => new CustomAttribute { key = p.Key, value = p.Value }).ToList() ??
                new List<CustomAttribute>();
            this.attachedEvents = attachedEvents?.Select(e => new EventReference(e)).ToList() ??
                new List<EventReference>();
        }

        public void OverrideProviders(string[] providersIds)
        {
            providers = providersIds.Select(id => new AnalyticsProviderId(id)).ToArray();
        }

        private IReadOnlyCollection<string> GetParameters()
        {
            List<string> result = new();
            if (observableParameters != null)
                result.AddRange(observableParameters.Select(p => p.ParameterId));
            if (categories != null)
                result.AddRange(categories.SelectMany(c => c.Parameters).Select(p => p.ParameterId));
            if (parameters != null)
                result.AddRange(parameters);

            return result;
        }
    }
}
