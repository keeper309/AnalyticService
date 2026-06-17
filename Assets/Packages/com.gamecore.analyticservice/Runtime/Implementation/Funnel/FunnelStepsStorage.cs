using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore.AnalyticService
{
    internal class FunnelStepsStorage : IFunnelStepsStorage
    {
        private readonly HashSet<string> _steps = new HashSet<string>();

        public IReadOnlyCollection<string> CompletedSteps => _steps;
        public string FunnelId { get; }

        public FunnelStepsStorage(string funnelId)
        {
            FunnelId = funnelId;
            ReadData();
        }

        public void Clear()
        {
            _steps.Clear();
            PlayerPrefs.DeleteKey(CreateKey());
        }

        public void AddStep(string stepId)
        {
            if (_steps.Add(stepId))
                SaveData();
        }

        public bool ContainsStep(string stepId)
        {
            return _steps.Contains(stepId);
        }

        private void ReadData()
        {
            string json = PlayerPrefs.GetString(CreateKey());
            if (!string.IsNullOrEmpty(json))
            {
                FunnelStepsData data = JsonUtility.FromJson<FunnelStepsData>(json);
                foreach (string step in data.steps)
                {
                    _steps.Add(step);
                }
            }
            else
            {
                _steps.Clear();
            }
        }

        private void SaveData()
        {
            string data = JsonUtility.ToJson(new FunnelStepsData { steps = _steps.ToArray() });
            PlayerPrefs.SetString(CreateKey(), data);
        }

        private string CreateKey()
        {
            return $"{AnalyticServiceConstants.FunnelEventPrefix}-{FunnelId}";
        }
    }
}