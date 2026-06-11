namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Analytic service settings.
    /// </summary>
    public class AnalyticServiceSettings
    {
        /// <summary>
        ///     Prevent exceptions from being thrown when parameter is not resolved.
        /// </summary>
        public bool SuppressExceptions { get; private set; }

        /// <summary>
        ///     Funnel event steps parameter id.
        /// </summary>
        public string FunnelEventStepParameter { get; private set; }

        /// <summary>
        ///     Funnel event parameter id.
        /// </summary>
        public string FunnelEvenParameter { get; private set; }

        public string FunnelEventIndexParameter { get; private set; }

        public AnalyticServiceSettings(
            bool suppressExceptions,
            string funnelEvenParameter = AnalyticServiceConstants.DefaultFunnelParameter,
            string funnelEventStepParameter = AnalyticServiceConstants.DefaultFunnelStepParameter,
            string funnelEventIndexParameter = AnalyticServiceConstants.DefaultFunnelIndexParameter
        )
        {
            FunnelEvenParameter = funnelEvenParameter;
            SuppressExceptions = suppressExceptions;
            FunnelEventStepParameter = funnelEventStepParameter;
            FunnelEventIndexParameter = funnelEventIndexParameter;
        }
    }
}