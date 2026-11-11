namespace FintechApp.Responses.TransactionReport
{
    /// <summary>
    /// One line of the profit and loss report: what came in, what went out to the network, what
    /// the provider paid us, the tax on both sides, and what is left.
    ///
    /// Every commission figure here is SIGNED, because in this system the sign is the information:
    /// a positive value is commission the house PAID OUT to that tier, a negative one is a
    /// surcharge the house COLLECTED from them. Splitting them into separate columns would double
    /// the width of the report and still leave the reader adding them up.
    ///
    /// The GST columns are kept apart from the money they are tax on for the same reason the
    /// daybook keeps them apart: the GST inside a charge WE collected is output tax we owe, while
    /// the GST inside a provider charge is input credit we claim. Netting them into one "GST"
    /// column is how a profit figure ends up including money that belongs to the government.
    /// </summary>
    public class ProfitLossViewModel
    {
        public int ID { get; set; }

        /// <summary>What this row groups - a date, a service, an operator or a user.</summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>The date, for a date-wise report. Null on the other groupings.</summary>
        public DateTime? Date { get; set; }

        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }

        /// <summary>
        /// Wallet balance at the start of the window, summed over the users in scope. Populated
        /// only for the date-wise and user-wise groupings - a balance has no meaning per service
        /// or per operator, and printing one there would invite it to be read as a float.
        /// </summary>
        public decimal Opening { get; set; }

        /// <summary>Funds loaded into the network in the window (fund transfers and approved requests).</summary>
        public decimal Purchase { get; set; }

        /// <summary>Value of the transactions that settled SUCCESS in the window.</summary>
        public decimal Sale { get; set; }

        /// <summary>Wallet balance at the end of the window. Same caveat as <see cref="Opening"/>.</summary>
        public decimal Closing { get; set; }

        /// <summary>Retailer tier: positive = commission paid to them, negative = surcharge collected.</summary>
        public decimal RetailerCommission { get; set; }
        public decimal DistributorCommission { get; set; }
        public decimal SuperDistributorCommission { get; set; }

        /// <summary>The three tiers added up - what the network cost us, net of what it paid us.</summary>
        public decimal TotalCommission { get; set; }

        /// <summary>
        /// GST on the downline side, as a positive amount: the output tax we owe on the charges we
        /// collected. GST is a tax amount rather than a direction, and the column name carries the
        /// direction - printed signed, a charge showed a negative tax, which reads as a refund.
        /// </summary>
        public decimal TotalGST { get; set; }

        /// <summary>What the provider paid us (positive) or charged us (negative), excluding GST.</summary>
        public decimal ProviderCommission { get; set; }

        /// <summary>
        /// GST on the provider side, as a positive amount: the input credit we may claim on what
        /// the provider charged us. Same reasoning as <see cref="TotalGST"/>.
        /// </summary>
        public decimal ProviderGST { get; set; }

        /// <summary>
        /// The margin: what the provider gave us less what we gave the network. This is the line
        /// the operator is really reading - a negative value means the service is sold at a loss.
        /// </summary>
        public decimal DiffCommission { get; set; }

        /// <summary>
        /// Input credit less output tax. Negative means net GST payable for the period, which is
        /// the usual case for a business that charges more tax than it is charged.
        /// </summary>
        public decimal DiffGST { get; set; }

        /// <summary>
        /// What the house keeps, from the figures that are income on BOTH sides: the provider
        /// commission net of its GST, less the commission paid down the network, plus the
        /// surcharge net of the GST we owe on it. Never the gross of either side.
        /// </summary>
        public decimal NetProfit { get; set; }
    }
}
