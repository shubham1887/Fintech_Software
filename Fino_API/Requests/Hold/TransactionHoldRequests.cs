namespace FintechApp.Requests.Hold
{
    /// <summary>Turn the global money-transfer hold on or off.</summary>
    public class SetTransactionHoldRequest
    {
        /// <summary>true = park new transfers as PENDING; false = resume and release the parked ones.</summary>
        public bool IsOn { get; set; }

        /// <summary>Why the hold was changed — stored on the setting so the change is accountable.</summary>
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>
    /// One transfer to let through now. Sent as a list, so the Pending DMT queue can release a
    /// single row or the rows an operator has ticked — the field name matches every other batch
    /// action on that screen.
    /// </summary>
    public class ReleaseHeldTransferRequest
    {
        public int TransactionID { get; set; }
    }
}
