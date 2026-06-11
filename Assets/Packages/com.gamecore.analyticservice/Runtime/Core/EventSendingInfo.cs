using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public struct EventSendingInfo
    {
        public string EventId { get; }
        public IReadOnlyCollection<string> Providers { get; }

        public EventSendingInfo(string eventId, IReadOnlyCollection<string> providers)
        {
            EventId = eventId;
            Providers = providers;
        }
    }
}
