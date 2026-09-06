namespace YESBankUAT
{
    internal class UserInput
    {
        public string MID { get; set; }
        public string MerchantKey { get; set; }

        public string MerchantVPA { get; set; }

        public string MerchantName { get; set; }

        public string MCC { get; set; }

        public string XIBMClientSecret { get; set; }
        public string XIBMClientID { get; set; }

        public string PrivateKeyFile { get; set; }
        public string PrivateKeyFilePassword { get; set; }

        public string AuthUserName { get; set; }
        public string AuthPassword { get; set; }
    }
}