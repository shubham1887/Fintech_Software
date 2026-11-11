namespace FintechApp.Requests.CashCollection
{
    /// <summary>
    /// Sets how much a user may owe at once. Zero means they may not take funds on credit at all —
    /// that is the default for a new account and the reason the Credit payment mode does not
    /// appear for them.
    /// </summary>
    public class SetCreditLimitRequest
    {
        public int UserID { get; set; }

        public decimal MaxOutstanding { get; set; }
    }
}
