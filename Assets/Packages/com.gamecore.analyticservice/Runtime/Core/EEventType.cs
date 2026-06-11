namespace GameCore.AnalyticService
{
    public enum EEventType
    {
        /// <summary>
        ///     Arbitrary event.
        /// </summary>
        Generic = 0,

        /// <summary>
        ///     Funnel event. Will be sent one time per step.
        /// </summary>
        Funnel = 1
    }
}