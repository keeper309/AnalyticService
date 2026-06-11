using System;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Add this attribute to the class that implements <see cref="IAnalyticsProvider" /> to specify the unique identifier of the provider.
    /// </summary>
    public class AnalyticsProviderIdAttribute : Attribute
    {
        public string ProviderId { get; }

        public bool AddOnImport { get; }

        public AnalyticsProviderIdAttribute(string providerId, bool addOnImport = true)
        {
            ProviderId = providerId;
            AddOnImport = addOnImport;
        }
    }
}
