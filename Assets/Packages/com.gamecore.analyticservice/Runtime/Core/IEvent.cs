using System;
using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public interface IEvent : IEquatable<IEvent>
    {
        /// <summary>
        ///     Parameters that should be ignored when merging events.
        /// </summary>
        HashSet<string> MergeIgnoreKeys { get; }

        /// <summary>
        ///     Id.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Event type.
        /// </summary>
        EEventType EventType { get; }

        /// <summary>
        ///     Duration of current analytics session.
        /// </summary>
        int TimeSpent { get; }

        /// <summary>
        ///     Event parameters.
        /// </summary>
        IReadOnlyDictionary<string, object> Parameters { get; }

        /// <summary>
        ///     Could contain arbitrary values. Could be used to store adjust event token for example.
        /// </summary>
        IReadOnlyDictionary<string, string> CustomAttributes { get; }

        /// <summary>
        ///     Providers that should handle this event.
        /// </summary>
        IReadOnlyCollection<string> Providers { get; }

        /// <summary>
        ///     Timestamp when the event was created.
        /// </summary>
        long CreatedAtTimestamp { get; }

        /// <summary>
        ///     Session id.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        ///     Nonce.
        /// </summary>
        string Nonce { get; }

        /// <summary>
        ///     Custom Event Deduplication ID.
        /// </summary>
        string Cedid { get; }

        /// <summary>
        ///     Adds parameters from the input event.
        /// </summary>
        /// <param name="event"></param>
        void Merge(IEvent @event);

        /// <summary>
        ///     Add parameter to the event.
        /// </summary>
        void AddParameter(string key, object value);

        /// <summary>
        ///     Apply parameters formatter to the event parameters.
        /// </summary>
        /// <param name="parametersFormatter"></param>
        void ReformatParameters(IParametersFormatter parametersFormatter);
    }
}
