using System.Globalization;

namespace MplanCore.Controller
{
    public static class StringExtension
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

        public static string ToTitleCase(this string source)
        {
            if (source == null)
                return "";

            TextInfo txtInfo = new CultureInfo("en-US", false).TextInfo;

            return txtInfo.ToTitleCase(source.Trim().ToLower());
        }

        public static string GetGUID(this string source)
        {
            return Guid.NewGuid().ToString();
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
    }
}