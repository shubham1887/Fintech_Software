namespace FintechApp.Responses.User
{
    /// <summary>
    /// One row of the hold report: a hold placed or released, on whose wallet, by whom, and why.
    /// </summary>
    public class WalletHoldHistoryViewModel
    {
        public int WalletHoldHistoryID { get; set; }
        public DateTime DOC { get; set; }

        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;

        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;

        /// <summary>HOLD or RELEASE.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Always positive — the Action says which direction it went.</summary>
        public decimal Amount { get; set; }

        /// <summary>Total held in that wallet after this change.</summary>
        public decimal HoldBalanceAfter { get; set; }

        /// <summary>The wallet's balance at the time, for context.</summary>
        public decimal WalletBalance { get; set; }

        public int ActionByUserID { get; set; }
        public string ActionByName { get; set; } = string.Empty;

        public string Remark { get; set; } = string.Empty;
    }

    public class WalletHoldHistoryWrapper
    {
        public int Count { get; set; }
        public List<WalletHoldHistoryViewModel> list { get; set; } = new();
    }
}
