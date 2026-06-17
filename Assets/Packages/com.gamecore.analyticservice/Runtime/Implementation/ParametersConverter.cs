using System;
using System.Globalization;

namespace GameCore.AnalyticService
{
    public static class ParametersConverter
    {
        public static bool AreEqualAsParameterValue(object a, object b)
        {
            return ToString(a).Equals(ToString(b));
        }

        public static string ToString(object value)
        {
            if (value == null)
                return "null";
            switch (value)
            {
                case bool boolValue: return boolValue ? "1" : "0";
                case IFormattable formattableValue: return formattableValue.ToString(null, CultureInfo.InvariantCulture);
                default: return value.ToString();
            }
        }

        public static object FromString(string value)
        {
            return value == "null" ? null : value;
        }
    }
}
