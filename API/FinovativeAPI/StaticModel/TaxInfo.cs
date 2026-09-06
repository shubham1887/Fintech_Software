namespace Project.StaticModel
{
    public static class TaxInfo
    {
        public const decimal TDS = 0.05M;
        public const decimal GST = 0.18M;
        public const decimal CCF = 0.01M;
        public const decimal Inclusion18Percentage = 0.1525M;//(18/118)

        public static readonly string DMT = "DMT";
        public static readonly string Prepaid = "Prepaid";
        public static readonly string Utility = "Utility";

        public static readonly string DMTInVoicePrefix = "D";
        public static readonly string PrepaidInVoicePrefix = "P";
        public static readonly string UtilityInVoicePrefix = "U";

        public static readonly string InVoice = "InVoice";
        public static readonly string SelfInvoice = "SelfInvoice";

        public static readonly string P2P = "P2P";
        public static readonly string P2ABankingRCM = "P2A Banking RCM";
        public static readonly string P2ABankingNonRCM = "P2A Banking Non RCM";
        public static readonly string P2ANonBanking = "P2A Non Banking";
    }
}