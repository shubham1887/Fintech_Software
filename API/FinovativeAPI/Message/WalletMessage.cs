namespace Project.Message
{
    public class WalletMessage
    {
        public static readonly string Invalid = "Please select valid wallet";
        public static readonly string InvalidWallet = "Invalid wallet";

        public static string HashBalanceMappingWithUser = "[UserID]#[WalletID]#[Balance]#[SystemName]";
    }
}