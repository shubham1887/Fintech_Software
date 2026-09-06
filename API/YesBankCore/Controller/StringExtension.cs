namespace YESBankCore.Controller
{
    internal static class StringExtension
    {
        public static string TrimIfNotNull(this string source)
        {
            if (source == null)
                return "";

            return source.Trim();
        }

        public static string TrimIfNotNullWithDefault(this string source, string defaultValue)
        {
            if (defaultValue == null)
                return "";

            defaultValue = defaultValue.Trim();

            if (source == null)
                return defaultValue;

            return defaultValue;
        }

        public static bool EqualsIgnoreCase(this string source, string secondString)
        {
            if (source == null)
                return false;

            if (secondString == null)
                return false;

            if (!source.Equals(secondString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static List<string> TrimAll(this List<string> list)
        {
            if (list == null) return null;
            return list.Select(s => s?.Trim()).ToList();
        }       
    }
}