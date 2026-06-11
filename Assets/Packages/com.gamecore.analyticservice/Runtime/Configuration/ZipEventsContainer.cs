using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GameCore.AnalyticService
{
    public class ZipEventsContainer
    {
        private const string Id = "id";

        public readonly Dictionary<string, ZipEvent> Events;
        public readonly HashSet<string> ObservableParameters;
        public readonly Dictionary<string, ZipParameters> Parameters;
        public readonly HashSet<string> Funnels;
        public readonly Dictionary<string, ZipFunnelSteps> FunnelSteps;

        public ZipEventsContainer(ZipArchive zip)
        {
            Dictionary<string, string> data = ExtractData(zip);
            Events = ParseEvents(data["Events.csv"]);
            ObservableParameters = ParseToHashSet(data["ObservableParameters.csv"], new[] { Id });
            Parameters = ParseParameters(data["Parameters.csv"]);

            Funnels = ParseToHashSet(data["Funnels.csv"], new[] { AnalyticServiceConstants.FunnelParameter, Id });
            FunnelSteps = ParseFunnelSteps(data["FunnelSteps.csv"]);
        }

        private Dictionary<string, ZipFunnelSteps> ParseFunnelSteps(string data)
        {
            string[] lines = data.Split('\n');

            List<(string funnelId, string index, string step)> steps =
                new List<(string funnelId, string index, string step)>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                if (cols.Contains(AnalyticServiceConstants.FunnelParameter))
                    continue;

                string funnelId = cols[0];
                string index = cols[1];
                string step = cols[2];

                steps.Add((funnelId, index, step));
            }

            Dictionary<string, ZipFunnelSteps> funnelSteps = steps.GroupBy(x => x.funnelId)
                .ToDictionary(
                    x => x.Key,
                    x => new ZipFunnelSteps(
                        x.Key,
                        x.Select(s => s.step).ToList(),
                        x.Select(s => s.index).ToList()
                    )
                );

            return funnelSteps;
        }

        private HashSet<string> ParseToHashSet(string data, IEnumerable<string> stringsToExclude = null)
        {
            if (stringsToExclude == null)
                stringsToExclude = new List<string>();

            return new HashSet<string>(
                data
                    .Split('\n')
                    .Select(s => s.Split(',')[0])
                    .Except(stringsToExclude)
                    .Select(s => s.Trim())
            );
        }

        private Dictionary<string, ZipParameters> ParseParameters(string parametersData)
        {
            string[] lines = parametersData.Split('\n');
            Dictionary<string, ZipParameters> parameters = new Dictionary<string, ZipParameters>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                string eventId = cols[0];

                HashSet<string> parametersSet = new HashSet<string>(
                    cols
                        .Skip(1)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.Trim())
                );

                parameters.Add(eventId, new ZipParameters(eventId, parametersSet));
            }

            return parameters;
        }

        private Dictionary<string, ZipEvent> ParseEvents(string eventsData)
        {
            string[] lines = eventsData.Split('\n');
            Dictionary<string, ZipEvent> events = new Dictionary<string, ZipEvent>();

            foreach (string line in lines)
            {
                string[] words = line.Split(',');

                if (words.Contains(Id))
                    continue;

                string id = words[0];
                string adjustToken = words[1];
                EventType type = ParseEventType(words[2]);

                string[] attachedEvents = words[3]
                    .Split('|')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                events.Add(id, new ZipEvent(id, adjustToken, type, attachedEvents));
            }

            return events;
        }

        private EventType ParseEventType(string type)
        {
            type = type.Trim();
            switch (type)
            {
                case AnalyticServiceConstants.EventTypeGeneric: return EventType.Generic;
                case AnalyticServiceConstants.EventTypeFunnel: return EventType.Funnel;
            }

            throw new ArgumentOutOfRangeException();
        }

        private Dictionary<string, string> ExtractData(ZipArchive archive)
        {
            Dictionary<string, string> data = archive.Entries.ToDictionary(x => x.Name, ExtractString);

            return data;
        }

        private string ExtractString(ZipArchiveEntry entry)
        {
            using (StreamReader sr = new StreamReader(entry.Open(), Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public class ZipEvent
        {
            public string Id { get; }
            public string AdjustToken { get; }
            public EventType Type { get; }
            public IReadOnlyCollection<string> AttachedEvents { get; }

            public ZipEvent(string id, string adjustToken, EventType type, IReadOnlyCollection<string> attachedEvents)
            {
                Id = id;
                AdjustToken = adjustToken;
                Type = type;
                AttachedEvents = attachedEvents;
            }
        }

        public class ZipParameters
        {
            public string EventId { get; }
            public HashSet<string> Parameters { get; }

            public ZipParameters(string eventId, HashSet<string> parameters)
            {
                EventId = eventId;
                Parameters = parameters;
            }
        }

        public class ZipFunnelSteps
        {
            public string FunnelId { get; }

            public IReadOnlyCollection<string> Indexes { get; }
            public IReadOnlyCollection<string> Steps { get; }

            public ZipFunnelSteps(
                string funnelId,
                IReadOnlyCollection<string> steps,
                IReadOnlyCollection<string> indexes
            )
            {
                FunnelId = funnelId;
                Steps = steps;
                Indexes = indexes;
            }
        }
    }
}
