using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    /// <summary>
    /// Implement to customize parameters reshaping logic, for example pack all parameters in one JSON.
    /// </summary>
    public interface IParametersFormatter
    {
        IReadOnlyDictionary<string, object> Format(IReadOnlyDictionary<string, object> parameters);
    }
}