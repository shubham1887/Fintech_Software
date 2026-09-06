namespace Project.Processing
{
    public static class OTPProcessing
    {
        internal static string GenerateOTP(out string OTP)
        {
            OTP = new Util().GetRandomOTP();
            string EncOTP = OTP;//new EncryptDecrypt().Encrypt(OTP);
            return EncOTP;
        }
    }
}