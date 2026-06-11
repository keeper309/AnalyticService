using UnityEngine;

namespace GameCore.AnalyticService
{
    [CreateAssetMenu(menuName = "Analytics/ParametersCategory", fileName = "ParametersCategory", order = 0)]
    public class ParametersCategory : ScriptableObject
    {
        [SerializeField] private Parameter[] parameters;
        public string CategoryName => name;
        public Parameter[] Parameters => parameters;
    }
}