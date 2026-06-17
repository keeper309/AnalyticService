using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public class JsonAllToOnePackingFormatter : IParametersFormatter
    {
        private const string Key = "payload";

        public IReadOnlyDictionary<string, object> Format(IReadOnlyDictionary<string, object> parameters)
        {
            return new Dictionary<string, object>
            {
                { "data", parameters }
            };
        }
    }

    public class EmptyParametersFormatter : IParametersFormatter
    {
        public IReadOnlyDictionary<string, object> Format(IReadOnlyDictionary<string, object> parameters)
        {
            return parameters;
        }
    }
}
