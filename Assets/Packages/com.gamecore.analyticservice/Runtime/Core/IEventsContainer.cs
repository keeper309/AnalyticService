using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Implement this interface to provide custom <see cref="IAnalyticsService" /> initialization.
    /// </summary>
    public interface IEventsContainer
    {
        /// <summary>
        ///     Collection of events description.
        /// </summary>
        IReadOnlyCollection<IEventProfile> EventProfiles { get; }

        /// <summary>
        ///     Collection of funnel profiles.
        /// </summary>
        IReadOnlyCollection<IFunnelProfile> FunnelProfiles { get; }

        /// <summary>
        ///     Settings for the analytic service.
        /// </summary>
        AnalyticServiceSettings Settings { get; }
    }
}