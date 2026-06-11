using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.AnalyticService
{
    [Serializable]
    public class EventReference
    {
        [SerializeField]
        private string eventId;

        public string EventId => eventId;

        public EventReference(string eventId)
        {
            this.eventId = eventId;
        }

        private IEnumerable<string> GetValues()
        {
            return EventsContainer.GetEventsId();
        }
    }
}