namespace FintechApp.Responses.CashCollection
{
    public class SelectByUserIDResponse
    {
        public string BalanceDetail { get; set; } = string.Empty;
        public decimal Outstanding { get; set; }

        /// <summary>The limit in force — this account's own, or the network default.</summary>
        public decimal MaxOutStanding { get; set; }

        /// <summary>
        /// True when the limit above is the network default rather than one set for this account,
        /// so a screen can say so instead of implying somebody chose this number here.
        /// </summary>
        public bool IsDefaultLimit { get; set; }

        /// <summary>Limit minus outstanding, floored at zero — how much more may be taken.</summary>
        public decimal AvailableCredit { get; set; }

        /// <summary>
        /// Who the money is owed TO. A retailer looking at their own outstanding needs to know who
        /// to pay; without it the figure is just a number they cannot act on.
        /// </summary>
        public int ParentID { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentMobile { get; set; } = string.Empty;

        /// <summary>Whose standing this is — the screen may be showing somebody else's.</summary>
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;

        /// <summary>False when cash collection is switched off for the install.</summary>
        public bool FeatureEnabled { get; set; }
    }
}
