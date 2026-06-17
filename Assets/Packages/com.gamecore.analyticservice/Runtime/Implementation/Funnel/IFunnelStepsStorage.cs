using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    internal interface IFunnelStepsStorage
    {
        IReadOnlyCollection<string> CompletedSteps { get; }
        string FunnelId { get; }
        void Clear();
        void AddStep(string stepId);
        bool ContainsStep(string stepId);
    }
}