using System;
using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    internal class StubFunnelStepsStorage : IFunnelStepsStorage
    {
        public IReadOnlyCollection<string> CompletedSteps => Array.Empty<string>();
        public string FunnelId => string.Empty;
        public void Clear() { }

        public void AddStep(string stepId) { }

        public bool ContainsStep(string stepId)
        {
            return false;
        }
    }
}