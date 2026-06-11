using System;
using UnityEngine;

namespace GameCore.AnalyticService
{
    /// <summary>
    ///     Reference to the parameter.
    /// </summary>
    [Serializable]
    public class Parameter
    {

        //todo: create drop list (GetValues)
        [SerializeField]
        private string parameterId;

        public string ParameterId => parameterId;

        public Parameter(string parameterId)
        {
            this.parameterId = parameterId;
        }

        private string[] GetValues()
        {
#if UNITY_EDITOR
            return EventsContainer.GetAvailableParameters();
#endif
#pragma warning disable 162
            return Array.Empty<string>();
#pragma warning restore 162
        }
    }
}
