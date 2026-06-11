using System.Text;

namespace GameCore.AnalyticService
{
    public static class JsonPrettifier
    {
        public static string PrettifyJson(string json, int indentSize)
        {
            StringBuilder sb = new();
            bool inQuotes = false;
            int indentLevel = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char current = json[i];
                char previous = i > 0 ? json[i - 1] : '\0';

                if (current == '\"' && previous != '\\')
                {
                    inQuotes = !inQuotes;
                    sb.Append(current);

                    continue;
                }

                if (!inQuotes)
                {
                    switch (current)
                    {
                        case '{':
                        case '[':
                            sb.Append(current);
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indentLevel * indentSize));

                            continue;

                        case '}':
                        case ']':
                            sb.AppendLine();
                            sb.Append(new string(' ', --indentLevel * indentSize));
                            sb.Append(current);

                            continue;

                        case ',':
                            sb.Append(current);
                            sb.AppendLine();
                            sb.Append(new string(' ', indentLevel * indentSize));

                            continue;

                        case ':':
                            sb.Append(current);
                            sb.Append(" ");

                            continue;

                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            continue;
                    }
                }

                sb.Append(current);
            }

            return sb.ToString();
        }

        public static string PrettifyNdjson(string ndjson, int indentSize = 4)
        {
            StringBuilder sb = new();
            string[] lines = ndjson.Split('\n');

            sb.AppendLine("["); // Начало массива

            for (int i = 0; i < lines.Length; i++)
            {
                string prettyLine = PrettifyJson(lines[i], indentSize);
                sb.Append(new string(' ', indentSize) + prettyLine.Trim());

                if (i < lines.Length - 1)
                    sb.Append(",");

                sb.AppendLine();
            }

            sb.Append("]"); // Закрываем массив

            return sb.ToString();
        }

        public static string NdjsonToJsonArray(string ndjson)
        {
            StringBuilder sb = new();
            string[] lines = ndjson.Split('\n');

            sb.AppendLine("["); // Начало массива

            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(lines[i].Trim());

                if (i < lines.Length - 1)
                    sb.Append(",");

                sb.AppendLine();
            }

            sb.Append("]"); // Закрываем массив

            return sb.ToString();
        }
    }
}
