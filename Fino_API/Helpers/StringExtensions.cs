namespace FintechApp.Helpers
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string source, string compare)
        {
            if (source == null && compare == null)
                return true;
            if (source == null || compare == null)
                return false;
            return source.Equals(compare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static string ToUpperTrim(this string source)
        {
            return source?.Trim().ToUpper() ?? string.Empty;
        }

        public static string ToLowerTrim(this string source)
        {
            return source?.Trim().ToLower() ?? string.Empty;
        }

        public static string ToTitleCase(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            return System.Globalization.CultureInfo.CurrentCulture
                         .TextInfo
                         .ToTitleCase(source.Trim().ToLower());
        }

        public static bool IsValidMobileNumber(this string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return false;

            mobileNumber = mobileNumber.Trim();
            return mobileNumber.Length == 10 && mobileNumber.All(char.IsDigit);
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string TrimIfNotNull(this string source)
        {
            if (source == null)
                return "";

            return source.Trim();
        }
    }
}