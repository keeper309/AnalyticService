using System;
using UnityEngine;

namespace GameCore.AnalyticService
{
    [Serializable]
    public class AnalyticsProviderId
    {

        [SerializeField]
        private string providerProviderId;

        public string ProviderId => providerProviderId;

        public AnalyticsProviderId(string providerProviderId)
        {
            this.providerProviderId = providerProviderId;
        }

        private string[] GetValues()
        {
#if UNITY_EDITOR
            return EventsContainer.GetAvailableProviders();
#endif

#pragma warning disable 162
            return Array.Empty<string>();
#pragma warning restore 162
        }
    }
}
