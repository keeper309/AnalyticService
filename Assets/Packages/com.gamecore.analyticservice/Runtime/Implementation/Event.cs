using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore.AnalyticService
{
    public class Event : IEvent
    {
        private Dictionary<string, object> _parameters = new();

        private readonly Dictionary<string, string> _customAttributes = new();

        public HashSet<string> MergeIgnoreKeys { get; } = new() { "cedid" };

        public string Id { get; }

        public EEventType EventType { get; }

        public int TimeSpent { get; }
        public IReadOnlyDictionary<string, object> Parameters => _parameters;

        public IReadOnlyDictionary<string, string> CustomAttributes => _customAttributes;
        public IReadOnlyCollection<string> Providers { get; }

        public long CreatedAtTimestamp { get; }
        public string SessionId { get; }

        public string Nonce { get; }

        public string Cedid { get; }

        public Event(
            string id,
            EEventType type,
            IReadOnlyCollection<string> providers,
            IReadOnlyDictionary<string, object> parameters,
            IReadOnlyDictionary<string, string> customAttributes,
            long createdAtTimestamp,
            string sessionId,
            string nonce,
            string cedid,
            int timeSpent
        )

        {
            Id = id;
            EventType = type;

            Providers = providers;

            CreatedAtTimestamp = createdAtTimestamp;

            SessionId = sessionId;

            Nonce = nonce;

            Cedid = cedid;

            TimeSpent = timeSpent;

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                _parameters.Add(parameter.Key, parameter.Value);
            }

            foreach (KeyValuePair<string, string> attribute in customAttributes)
            {
                _customAttributes.Add(attribute.Key, attribute.Value);
            }
        }

        public void Merge(IEvent @event)
        {
            foreach (KeyValuePair<string, object> parameter in @event.Parameters)
            {
                if (MergeIgnoreKeys.Contains(parameter.Key))
                    continue;

                _parameters[parameter.Key] = parameter.Value;
            }
        }

        public void AddParameter(string key, object value)
        {
            _parameters[key] = value;
        }

        public void ReformatParameters(IParametersFormatter parametersFormatter)
        {
            IReadOnlyDictionary<string, object> parameters = parametersFormatter.Format(_parameters);
            if (parameters is Dictionary<string, object> dictionary)
            {
                _parameters = dictionary;
            }
            else
            {
                _parameters = parameters.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public bool Equals(IEvent other)
        {
            return Equals((Event)other);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Event)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, EventType);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder
                .Append("Analytic Event:\n")
                .Append($"Type: {EventType},\n")
                .Append($"Id: {Id},")
                .Append("Parameters:,\n");

            if (Parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in Parameters)
                {
                    stringBuilder.Append($"Key: {pair.Key}, Value: {pair.Value}\n");
                }
            }

            stringBuilder.Append("CustomAttributes:,\n");
            if (CustomAttributes != null)
            {
                foreach (KeyValuePair<string, string> pair in CustomAttributes)
                {
                    stringBuilder.Append($"Key: {pair.Key}, Value: {pair.Value}\n");
                }
            }

            stringBuilder.Append("Providers:,\n");

            if (Providers != null)
            {
                foreach (string provider in Providers)
                {
                    stringBuilder.Append($"Provider: {provider}\n");
                }
            }


            return stringBuilder.ToString();
        }

        private bool Equals(Event other)
        {
            return Id == other.Id &&
                EventType == other.EventType &&
                TimeSpent == other.TimeSpent &&
                CreatedAtTimestamp == other.CreatedAtTimestamp &&
                SessionId == other.SessionId &&
                Nonce == other.Nonce &&
                Cedid == other.Cedid &&
                ParametersAreEqual(_parameters, other._parameters) &&
                CustomAttributesAreEqual(_customAttributes, other._customAttributes) &&
                ProvidersAreEqual(Providers, other.Providers);
        }

        private bool ParametersAreEqual(IReadOnlyDictionary<string, object> first, IReadOnlyDictionary<string, object> second)
        {
            if (first.Count != second.Count)
                return false;

            foreach (KeyValuePair<string, object> pair in first)
            {
                if (!second.TryGetValue(pair.Key, out object value))
                    return false;

                if (!ParametersConverter.AreEqualAsParameterValue(pair.Value, value))
                    return false;
            }

            return true;
        }

        private bool CustomAttributesAreEqual(IReadOnlyDictionary<string, string> first, IReadOnlyDictionary<string, string> second)
        {
            if (first.Count != second.Count)
                return false;

            foreach (KeyValuePair<string, string> pair in first)
            {
                if (!second.TryGetValue(pair.Key, out string value))
                    return false;

                if (pair.Value != value)
                    return false;
            }

            return true;
        }

        private bool ProvidersAreEqual(IReadOnlyCollection<string> first, IReadOnlyCollection<string> second)
        {
            if (first.Count != second.Count)
                return false;

            foreach (string provider in first)
            {
                if (!second.Contains(provider))
                    return false;
            }

            return true;
        }
    }
}
