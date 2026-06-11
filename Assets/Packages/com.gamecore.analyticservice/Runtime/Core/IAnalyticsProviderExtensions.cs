namespace GameCore.AnalyticService
{
    public static class IAnalyticsProviderExtensions
    {
        public static ProviderInfo GetProviderInfo(this IAnalyticsProvider provider)
        {
            return new ProviderInfo { Id = provider.Id };
        }
    }
}
