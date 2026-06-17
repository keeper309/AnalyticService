using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GameCore.GeneralExtensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    public class EventsCache : IEventsCache
    {
        private readonly ILogger _logger;
        private readonly LinkedList<IEvent> _cachedEvents;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private bool _isDirty;
        private readonly object _lock = new();

        private int MaxCacheSize { get; }

        private string SaveDirectory { get; }

        private string SaveFileName { get; }

        private string SaveFilePath { get; }

        public IEnumerable<IEvent> Events => _cachedEvents;
        public int EventsCount => _cachedEvents.Count;

        public EventsCache(ILogger logger, string id, int maxCacheSize)
        {
            _logger = logger;
            MaxCacheSize = maxCacheSize;
            SaveDirectory = Application.persistentDataPath + "/AnalyticsCache";
            SaveFileName = $"cache-{id}.json";
            SaveFilePath = Path.Combine(SaveDirectory, SaveFileName);
            _cachedEvents = new LinkedList<IEvent>();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _isDirty = true;
                _cachedEvents.Clear();

                FileExtensions.TryDeleteFile(SaveFilePath, out _);
            }
        }

        public void Add(IEvent @event)
        {
            lock (_lock)
            {
                _isDirty = true;
                _cachedEvents.AddLast(@event);
                EnsureCacheSize();
            }
        }

        public void Remove(IEvent @event)
        {
            lock (_lock)
            {
                _isDirty = true;
                _cachedEvents.Remove(@event);
            }
        }

        public void Add(IReadOnlyCollection<IEvent> events)
        {
            lock (_lock)
            {
                _isDirty = true;
                foreach (IEvent @event in events)
                {
                    _cachedEvents.AddLast(@event);
                }

                EnsureCacheSize();
            }
        }

        public void RemoveEvents(IReadOnlyCollection<IEvent> events)
        {
            lock (_lock)
            {
                _isDirty = true;
                foreach (IEvent @event in events)
                {
                    _cachedEvents.Remove(@event);
                }
            }
        }

        public void Dispose()
        {
            _fileLock?.Dispose();
        }

        public async UniTask LoadAsync()
        {
            await _fileLock.WaitAsync();
            CacheStorage storage = null;
            try
            {
                await UniTask.RunOnThreadPool(() => storage = Load());
                if (storage != null)
                {
                    lock (_lock)
                    {
                        foreach (IEvent @event in from eventData in storage.events select eventData.ToEvent())
                        {
                            _cachedEvents.AddLast(@event);
                        }
                    }
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async UniTask SaveAsync()
        {
            if (!_isDirty)
                return;
            await _fileLock.WaitAsync();
            CacheStorage storage = null;
            lock (_lock)
            {
                storage = new CacheStorage { events = _cachedEvents.Select(x => new EventData(x)).ToList() };
            }
            try
            {
                await UniTask.RunOnThreadPool(() => Save(storage));
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
            finally
            {
                _isDirty = false;
                _fileLock.Release();
            }
        }

        public void Save()
        {
            if (!_isDirty)
                return;

            _fileLock.Wait();
            CacheStorage storage = null;
            lock (_lock)
            {
                storage = new CacheStorage { events = _cachedEvents.Select(x => new EventData(x)).ToList() };
            }
            try
            {
                Save(storage);
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
            finally
            {
                _isDirty = false;
                _fileLock.Release();
            }
        }

        private CacheStorage Load()
        {
            if (!File.Exists(SaveFilePath))
            {
                _cachedEvents.Clear();

                return null;
            }

            CacheStorage storage = null;
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                storage = JsonUtility.FromJson<CacheStorage>(json);
            }
            catch (Exception e)
            {
                storage = null;
                _logger.Exception(e);
            }

            return storage;
        }

        private void EnsureCacheSize()
        {
            while (_cachedEvents.Count > MaxCacheSize)
            {
                _cachedEvents.RemoveFirst();
            }
        }

        private void Save(CacheStorage storage)
        {
            try
            {
                FileExtensions.TryCreateDirectory(SaveDirectory, out _);
                string json = JsonUtility.ToJson(storage);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
        }

        [Serializable]
        private class CacheStorage
        {
            public List<EventData> events;
        }

        [Serializable]
        private class EventData
        {
            public string id;
            public int eventType;
            public List<StringPair> parameters;
            public List<StringPair> customAttributes;
            public List<string> providers;
            public long createdAtTimestamp;
            public string sessionId;
            public string nonce;
            public string cedid;
            public int timeSpent;

            public EventData(IEvent @event)
            {
                FromEvent(@event);
            }

            public void FromEvent(IEvent @event)
            {
                id = @event.Id;
                eventType = (int)@event.EventType;
                parameters = @event.Parameters
                    .Select(x => new StringPair { key = x.Key, value = ParametersConverter.ToString(x.Value) })
                    .ToList();
                customAttributes = @event.CustomAttributes.Select(x => new StringPair { key = x.Key, value = x.Value }).ToList();
                providers = @event.Providers.ToList();
                createdAtTimestamp = @event.CreatedAtTimestamp;
                sessionId = @event.SessionId;
                nonce = @event.Nonce;
                cedid = @event.Cedid;
                timeSpent = @event.TimeSpent;
            }

            public IEvent ToEvent()
            {
                Event result = new(
                    id,
                    (EEventType)eventType,
                    providers,
                    parameters.ToDictionary(x => x.key, x => ParametersConverter.FromString(x.value)),
                    customAttributes.ToDictionary(x => x.key, x => x.value),
                    createdAtTimestamp,
                    sessionId,
                    nonce,
                    cedid,
                    timeSpent
                );

                return result;
            }

            [Serializable]
            public class StringPair
            {
                public string key;
                public string value;
            }
        }
    }
}
