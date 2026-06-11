using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public class FunnelCompletionReport
    {
        public string FunnelId { get; }
        public IReadOnlyCollection<string> CompletedSteps { get; }

        public FunnelCompletionReport(string funnelId, IReadOnlyCollection<string> completedSteps)
        {
            FunnelId = funnelId;
            CompletedSteps = completedSteps;
        }
    }
}