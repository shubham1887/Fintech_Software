namespace FintechApp.Responses.WalletToWalletTransfer
{
    public class WalletTransferResponse
    {
        // Kept under their original names because the Android build and the legacy panel read them:
        // on a generalised transfer they carry the SOURCE and DESTINATION closing balance.
        public string AepsWalletBal { get; set; } = string.Empty;
        public string mainWalletBal { get; set; } = string.Empty;

        // The same two balances, named for what they actually are, plus the wallets involved —
        // so a receipt can say "Recharge → DMT" instead of assuming the source was AEPS.
        public string FromWalletName { get; set; } = string.Empty;
        public string ToWalletName { get; set; } = string.Empty;
        public decimal FromWalletBalance { get; set; }
        public decimal ToWalletBalance { get; set; }
        public int ReferenceID { get; set; }
        public string Remark { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DateTime { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
    /// <summary>
    /// One of the caller's own wallets, for the From / To pickers on the transfer screen. Label is
    /// pre-composed ("Recharge — ₹1,490.00") so the dropdown shows the balance a user is about to
    /// move money out of, which is the one number that decides the amount they type.
    /// </summary>
    public class MyWalletResponse
    {
        public int WalletID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal HoldBalance { get; set; }
        public string Label { get; set; } = string.Empty;
    }

}
