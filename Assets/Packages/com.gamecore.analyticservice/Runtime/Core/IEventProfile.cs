using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Event description.
    /// </summary>
    public interface IEventProfile
    {
        // bool SendToAllProviders { get; }

        IReadOnlyCollection<string> Providers { get; }

        /// <summary>
        ///     Event type.
        /// </summary>
        EEventType EventType { get; }

        /// <summary>
        ///     Id.
        /// </summary>
        string EventId { get; }

        /// <summary>
        ///     Collection of parameter ids.
        /// </summary>
        IReadOnlyCollection<string> Parameters { get; }

        /// <summary>
        ///     Could contain arbitrary values. Could be used to store adjust event token for example.
        /// </summary>
        IReadOnlyDictionary<string, string> CustomAttributes { get; }

        /// <summary>
        ///     Event ids to call after this event.
        /// </summary>
        IReadOnlyCollection<string> AttachedEvents { get; }

        void OverrideProviders(string[] providersIds);
    }
}
