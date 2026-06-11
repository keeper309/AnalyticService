using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Provides access to values.
    /// </summary>
    public interface IAnalyticsParametersProvider
    {
        /// <summary>
        ///     Collection of provided parameters.
        /// </summary>
        IReadOnlyCollection<string> ParametersId { get; }

        /// <summary>
        ///     Provides values.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetValue(string key);

        /// <summary>
        ///     Initialize.
        /// </summary>
        void Initialize();
    }
}