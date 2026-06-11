using System;
using System.Collections.Generic;
using GameCore.GeneralExtensions;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Implement this interface to provide access to analytics libraries (Adjust, FB) functionality.
    /// </summary>
    public interface IAnalyticsProvider : IInitializable, IDisposable
    {
        /// <summary>
        ///     Reports that event was sent correctly.
        /// </summary>
        event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsSuccess;

        /// <summary>
        ///     Reports that event failed to send.
        /// </summary>
        event Action<IAnalyticsProvider, IReadOnlyList<IEvent>> OnEventsFailed;

        /// <summary>
        ///     Unique identifier of the provider.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Provider custom attributes. Use to get custom data from the provider, such ass Adjust app token.
        /// </summary>
        IReadOnlyDictionary<string, string> CustomAttributes { get; }

        /// <summary>
        ///     Send event.
        /// </summary>
        /// <param name="event"></param>
        void SendEvent(IEvent @event);

        void Construct(IAppLifecycleProvider appLifecycleProvider, AnalyticsSessionWatcher analyticsSessionWatcher);
    }
}
