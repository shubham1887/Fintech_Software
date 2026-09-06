namespace Project.Message
{
    public class APIUserMessage
    {
        public static readonly string WalletNotMapp = "Wallet not mapped with service, please contact customer care";
        public static readonly string InValidWallet = "Invalid Wallet";
        public static readonly string InsufficientBalance = "Insufficient Balance";
        public static readonly string CommissionNotSet = "Slab commission not set for you, please contact customer care";
        public static readonly string ChannelCommissionNotSet = "Slab commission not set for you, please contact to Distributor or SuperDistributor";
        public static readonly string InsufficientBalancewithCommi = "Insufficient Balance, due to commission";
        public static readonly string ValidationSucess = "Validation done successfully";
        public static readonly string AmountShouldGreterThan10 = "Invalid amount, Should greater than 10";
        public static readonly string CallbackSucess = "Callback send successfully";
        public static readonly string TansactionInProcess = "Transaction in process";
        public static readonly string NotAPIUser = "This is not api user transaction";
        public static readonly string BankDown = "Bank is down, try after sometime.";
        public static readonly string LowBalance = "Error LB occurred, please contact customer care";
        public static readonly string BalanceCheckHitLimit = "Balance check limit is over. Please, try after sometime.";
        public static readonly string StatusCheckHitLimit = "Status check limit is over. Please, try after sometime.";
    }
}