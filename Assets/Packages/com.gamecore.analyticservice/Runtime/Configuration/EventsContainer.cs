using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameCore.AnalyticService
{
    [CreateAssetMenu(menuName = "Analytics/EventsContainer", fileName = "EventsContainer", order = 0)]
    public partial class EventsContainer : ScriptableObject, IEventsContainer
    {
        [SerializeField]
        public bool suppressExceptions;

        [SerializeField]
        public string funnelParameter = AnalyticServiceConstants.DefaultFunnelParameter;

        [SerializeField]
        public string funnelIndexParameter = AnalyticServiceConstants.DefaultFunnelIndexParameter;

        [SerializeField]
        public string funnelStepParameter = AnalyticServiceConstants.DefaultFunnelStepParameter;

        [SerializeField]
        public List<EventReference> usedEvents;

        [SerializeField]
        public List<EventProfile> eventProfiles = new();

        [SerializeField]
        public List<FunnelProfile> funnelProfiles = new();

        [SerializeField]
        public List<ParametersCategory> categories = new();

        private string _notImplementedParameters;
        private bool _hasNotImplementedParameters;

        public IReadOnlyCollection<ParametersCategory> ParametersCategories => categories;
        public IReadOnlyCollection<IEventProfile> EventProfiles => GetUsedProfiles();
        public IReadOnlyCollection<IFunnelProfile> FunnelProfiles => funnelProfiles;

        public AnalyticServiceSettings Settings => new(
            suppressExceptions,
            funnelParameter,
            funnelStepParameter,
            funnelIndexParameter
        );

        public static string[] GetAvailableProviders()
        {
#if UNITY_EDITOR

            return GetNonAbstractDerivedTypes(typeof(IAnalyticsProvider))
                .Select(GetAttribute)
                .Where(p => p != null && !string.IsNullOrEmpty(p.ProviderId) && p.AddOnImport)
                .Select(a => a.ProviderId)
                .ToArray();

#endif

#pragma warning disable 162
            return Array.Empty<string>();
#pragma warning restore 162

            AnalyticsProviderIdAttribute GetAttribute(Type type)
            {
                if (type.GetCustomAttributes(typeof(AnalyticsProviderIdAttribute), true).Length == 0)
                {
                    return null;
                }

                AnalyticsProviderIdAttribute attribute =
                    (AnalyticsProviderIdAttribute)type.GetCustomAttributes(typeof(AnalyticsProviderIdAttribute), true)[0];

                return attribute;
            }
        }

        public static string[] GetAvailableParameters()
        {
#if UNITY_EDITOR

            return GetNonAbstractDerivedTypes(typeof(IAnalyticsParametersProvider))
                .Select(GetParameters)
                .SelectMany(p => p)
                .ToArray();
#endif

#pragma warning disable 162
            return Array.Empty<string>();
#pragma warning restore 162

            string[] GetParameters(Type type)
            {
                if (type.GetCustomAttributes(typeof(ParametersAttribute), true).Length == 0)
                {
                    return Array.Empty<string>();
                }

                ParametersAttribute attribute =
                    (ParametersAttribute)type.GetCustomAttributes(typeof(ParametersAttribute), true)[0];

                return attribute.Parameters;
            }
        }

        public static IEnumerable<string> GetEventsId()
        {
#if UNITY_EDITOR

            if (Selection.activeObject is EventsContainer container)
            {
                return container.eventProfiles.Select(p => p.EventId).ToArray();
            }

#endif

            return Array.Empty<string>();
        }

        private void OnValidate()
        {
            ValidateImplementation();
        }

        private bool HasNotImplementedParameters()
        {
            return _hasNotImplementedParameters;
        }

        private void ValidateImplementation()
        {
            string[] declaredParameters = eventProfiles
                .SelectMany(profile => profile.observableParameters.Select(op => op.ParameterId))
                .ToArray();
            string[] availableParameters = GetAvailableParameters();
            string[] notImplemented = declaredParameters.Except(availableParameters).ToArray();
            _hasNotImplementedParameters = notImplemented.Length > 0;
            _notImplementedParameters = string.Join(", ", notImplemented);
        }

        private IReadOnlyCollection<IEventProfile> GetUsedProfiles()
        {
            List<string> usedProfilesIds = usedEvents.Select(e => e.EventId).ToList();

            return eventProfiles.Where(p => usedProfilesIds.Contains(p.EventId)).ToList();
        }
    }
}
