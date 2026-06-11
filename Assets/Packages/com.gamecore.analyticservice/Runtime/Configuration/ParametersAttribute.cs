using System;

namespace GameCore.AnalyticService
{
    /// <summary>
    /// Add this attribute to the class that implements <see cref="IAnalyticsParametersProvider"/> to show the parameters in <see cref="EventsContainer"/>.
    /// </summary>
    public class ParametersAttribute : Attribute
    {
        public string[] Parameters { get; }

        public ParametersAttribute(string[] parameters)
        {
            Parameters = parameters;
        }
    }
}