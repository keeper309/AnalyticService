using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameCore.GeneralExtensions;
using UnityEditor;
using UnityEngine;

namespace GameCore.AnalyticService
{
    [Serializable]
    public class FunnelProfile : IFunnelProfile
    {
        [SerializeField] public string funnelId;

        [SerializeField] public List<StepIndexPair> funnelSteps;

        [SerializeField] private TextAsset constantsFileAsset;
        public string FunnelId => funnelId;
        public IReadOnlyCollection<StepIndexPair> FunnelSteps => funnelSteps;

        public FunnelProfile(string funnelId, string[] funnelSteps = null, string[] indexes = null)
        {
            this.funnelId = funnelId;

            if (funnelSteps == null || indexes == null)
                return;

            if (funnelSteps.Length != indexes.Length)
            {
                throw new ArgumentException("Funnel steps and indexes arrays must have the same length.");
            }

            this.funnelSteps = new List<StepIndexPair>();

            for (int i = 0; i < funnelSteps.Length; i++)
            {
                this.funnelSteps.Add(
                    new StepIndexPair
                    {
                        step = funnelSteps[i],
                        index = indexes[i]
                    }
                );
            }
        }

#if UNITY_EDITOR

        //todo: inspector button
        private void GenerateFunnelStepsConstants()
        {
            string className = $"FunnelStepsConstants{funnelId.ToPascalCase()}";

            string constantsFilePath = AssetDatabase.GetAssetPath(constantsFileAsset);
            string path = EventsContainer.GenerateConstantsClass(
                constantsFilePath,
                className,
                constantsFileAsset,
                funnelSteps.Select(s => s.step).ToArray()
            );

            if (constantsFileAsset != null)
                return;
            constantsFileAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (!(Selection.activeObject is EventsContainer container))
                return;
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
