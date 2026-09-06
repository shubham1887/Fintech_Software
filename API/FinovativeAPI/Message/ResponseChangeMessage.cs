namespace Project.Message
{
    public class ResponseChangeMessage
    {
        public static readonly string RazorpayInsuffisentBalance = "PAYOUT FAILED DUE TO INSUFFICIENT FUNDS IN THE BANK ACCOUNT.";
        public static readonly string RazorpayNotAvailableBalance = "Not enough available balance in the account";
        public static readonly string InsufficientWalletBalance = "INSUFFICIENT WALLET BALANCE";
        public static readonly string DoNotHaveSufficientBalance = "YOU DO NOT HAVE SUFFICIENT BALANCE TO PERFORM THIS TRANSACTION!";
        public static readonly string InsufficientBalance = "Insufficient Balance";
        public static readonly string LevinDMTLowBalance = "Insufficient balance or transaction amount should be minimum Rs.100 and Maximum 5000";
    }
}
