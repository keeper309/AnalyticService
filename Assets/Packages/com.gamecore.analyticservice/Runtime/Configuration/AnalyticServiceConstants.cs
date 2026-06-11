namespace GameCore.AnalyticService
{
    public static class AnalyticServiceConstants
    {
        // never change this value
        public const string FunnelEventPrefix = "funnel_event_pass_mark";
        public const string DefaultFunnelStepParameter = "step_id";
        public const string DefaultFunnelParameter = "funnel_id";
        public const string DefaultFunnelIndexParameter = "index";

        public const string EventTypeGeneric = "Generic";
        public const string EventTypeFunnel = "Funnel";

        public static string FunnelIndexParameter { get; internal set; } = DefaultFunnelIndexParameter;
        public static string FunnelStepParameter { get; internal set; } = DefaultFunnelStepParameter;
        public static string FunnelParameter { get; internal set; } = DefaultFunnelParameter;
    }
}