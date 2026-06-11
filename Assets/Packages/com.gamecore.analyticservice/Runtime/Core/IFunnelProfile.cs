using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public interface IFunnelProfile
    {
        string FunnelId { get; }
        IReadOnlyCollection<StepIndexPair> FunnelSteps { get; }
    }
}