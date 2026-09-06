namespace FintechApp.Helpers
{
    public static class OtpHelper
    {
        private static readonly Random _rng = new();

        public static string GenerateOtp(int length = 6)
        {
            const string digits = "1234567890";
            return new string(Enumerable.Repeat(digits, length)
                .Select(s => s[_rng.Next(s.Length)]).ToArray());
        }

        public static string GetRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_rng.Next(s.Length)]).ToArray());
        }

        public static string GetUniqueRandomNumber(int length = 15)
        {
            var digits = Guid.NewGuid().ToString()
                .Where(char.IsDigit).ToArray();
            var num = new string(digits).TrimStart('0');
            return num.Length >= length ? num[..length] : num;
        }
    }
}
