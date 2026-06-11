using System;
using System.Collections.Generic;
using GameCore.GeneralExtensions;

namespace GameCore.AnalyticService
{
    public interface IAnalyticsService : IInitializable, IDisposable
    {
        event Action<EventSendingInfo> OnEventSent;
        IReadOnlyCollection<ProviderInfo> AvailableProviders { get; }

        /// <summary>
        ///     Collection of registered events.
        /// </summary>
        IReadOnlyCollection<IEventProfile> EventProfiles { get; }

        /// <summary>
        ///     Collection of registered funnels.
        /// </summary>
        IReadOnlyCollection<IFunnelProfile> FunnelProfiles { get; }

        /// <summary>
        ///     Overwrite funnel completion local data with the provided report.
        /// </summary>
        /// <param name="report"></param>
        void ApplyFunnelCompletionReport(FunnelCompletionReport report);

        /// <summary>
        ///     Overwrite funnels completion local data with the provided reports.
        /// </summary>
        /// <param name="reports"></param>
        void ApplyFunnelCompletionReports(IReadOnlyCollection<FunnelCompletionReport> reports);

        /// <summary>
        ///     Provides funnels report.
        /// </summary>
        IReadOnlyCollection<FunnelCompletionReport> GetFunnelsReport();

        /// <summary>
        ///     Send funnel event.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="funnelId"></param>
        /// <param name="stepId"></param>
        /// <param name="additionalValues"></param>
        /// <param name="formatter"></param>
        EventSendingInfo SendFunnelEvent(
            string eventId,
            string funnelId,
            string stepId,
            IDictionary<string, object> additionalValues = null,
            IParametersFormatter formatter = null
        );

        /// <summary>
        ///     Send event.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="additionalValues"></param>
        /// <param name="parametersFormatter"></param>
        EventSendingInfo SendEvent(
            string id,
            IDictionary<string, object> additionalValues = null,
            IParametersFormatter parametersFormatter = null
        );

        /// <summary>
        ///     Clears funnel events cache. All funnel event steps could be sent again.
        /// </summary>
        void ClearFunnel();

        /// <summary>
        ///     Clears funner events cache for particular funnel.
        /// </summary>
        /// <param name="funnelId"></param>
        void ClearFunnel(string funnelId);

        /// <summary>
        ///     Register new event. Could be invoked only before <see cref="IInitializable.Initialize" />, otherwise runtime
        ///     exception will be thrown.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        IAnalyticsService RegisterEventProfile(IEventProfile profile);

        /// <summary>
        ///     Register new parameters provider. Could be invoked only before <see cref="IInitializable.Initialize" />, otherwise
        ///     runtime exception will be thrown.
        /// </summary>
        /// <param name="parametersProvider"></param>
        /// <returns></returns>
        IAnalyticsService RegisterParameterProvider(IAnalyticsParametersProvider parametersProvider);

        /// <summary>
        ///     Register new analytics provider. Could be invoked only before <see cref="IInitializable.Initialize" />, otherwise
        ///     runtime exception will be thrown.
        /// </summary>
        /// <param name="analyticsProvider"></param>
        /// <returns></returns>
        IAnalyticsService RegisterAnalyticsProvider(IAnalyticsProvider analyticsProvider);

        /// <summary>
        ///     Register new funnel. Could be invoked only before <see cref="IInitializable.Initialize" />, otherwise
        ///     runtime exception will be thrown.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        IAnalyticsService RegisterFunnelProfile(IFunnelProfile profile);
    }
}
