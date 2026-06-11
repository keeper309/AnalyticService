using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameCore.AnalyticService
{
    public interface IEventsCache : IDisposable
    {
        IEnumerable<IEvent> Events { get; }

        int EventsCount { get; }
        void Clear();
        void Add(IEvent @event);

        void Remove(IEvent @event);

        void Add(IReadOnlyCollection<IEvent> events);
        void RemoveEvents(IReadOnlyCollection<IEvent> events);

        UniTask SaveAsync();

        UniTask LoadAsync();

        void Save();
    }
}
