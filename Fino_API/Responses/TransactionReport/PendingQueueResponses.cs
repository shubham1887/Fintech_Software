namespace FintechApp.Responses.TransactionReport
{
    /// <summary>
    /// One pending queue - a service kind that is configured on this install, and what is waiting
    /// in it. The screen builds itself from these rather than from a compiled-in list of two, so a
    /// newly-sold service gets a working queue the moment the admin creates it.
    /// </summary>
    public class PendingQueueItem
    {
        /// <summary>ServiceKinds constant, echoed back as ServiceType to list this queue.</summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>What to call it on screen - the service's own name where there is only one.</summary>
        public string Label { get; set; } = string.Empty;

        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }

        /// <summary>When the oldest waiting transaction was created. Null when nothing is waiting.</summary>
        public DateTime? OldestPendingOn { get; set; }

        /// <summary>
        /// How long the oldest has been waiting, in hours. The number that decides whether a queue
        /// needs attention: twenty transactions from the last five minutes are a provider being
        /// slow, and one from yesterday is money nobody has accounted for.
        /// </summary>
        public double OldestPendingHours { get; set; }

        /// <summary>The services behind this kind, for a screen that wants to narrow further.</summary>
        public List<int> ServiceIDs { get; set; } = new();
    }

    /// <summary>Every pending queue on this install, plus the totals across them.</summary>
    public class PendingQueueSummary
    {
        public List<PendingQueueItem> Queues { get; set; } = new();
        public int TotalPending { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
