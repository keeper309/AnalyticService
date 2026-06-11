using GameCore.GeneralExtensions;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AnalyticService
{
    public class ConstantsClassCodeGenerator
    {
        public string Generate(string className, IEnumerable<string> constants)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"public static class {className}\n{{");

            foreach (string constant in constants)
            {
                sb.Append(ToConstString(constant));
            }

            sb.Append("\n}");

            return sb.ToString();
        }

        private string ToConstString(string constant)
        {
            string constantName = constant.IsFirstLetterNumber() ? $"D{constant}" : constant;

            return $"\n    public const string {constantName.ToPascalCase()} = \"{constant}\";";
        }
    }
}