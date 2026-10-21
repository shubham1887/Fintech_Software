namespace FintechApp.Responses.TaxInvoice
{
    /// <summary>What a detection run found and created.</summary>
    public class ExchangeLegDetectionResult
    {
        public bool DryRun { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        public int OutwardLegsCreated { get; set; }
        public int InwardLegsCreated { get; set; }

        /// <summary>Movements that already had a leg. Large on a re-run, zero on a first run.</summary>
        public int LegsAlreadyKnown { get; set; }

        /// <summary>Partners whose legs could not be classified, each named in Warnings.</summary>
        public int SkippedUnresolved { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// One partner's position for a period.
    ///
    /// NetPosition is the number that makes a swap feel like an adjustment — it comes to zero when
    /// both sides credited each other equally. TotalTurnover is the number GST cares about: under
    /// Section 7(1)(a) barter is supply and EACH leg is a taxable supply at open market value, so
    /// the turnover is the sum of the two legs, not the difference between them. Five partners
    /// swapping Rs 50 lakh a month is Rs 3 crore of turnover, and it leaves the books entirely if
    /// only the incoming credit was ever recorded.
    /// </summary>
    public class ExchangeRegisterRow
    {
        public int PartnerUserID { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string PartnerGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>We credited them — our supply, our invoice.</summary>
        public decimal OutwardAmount { get; set; }
        public int OutwardLegs { get; set; }

        /// <summary>They credited us — their supply, their invoice, which we have to obtain.</summary>
        public decimal InwardAmount { get; set; }
        public int InwardLegs { get; set; }

        /// <summary>Outward minus inward. Near zero on a pure swap, and misleading on its own.</summary>
        public decimal NetPosition { get; set; }

        /// <summary>Outward plus inward. The figure that belongs in the return.</summary>
        public decimal TotalTurnover { get; set; }

        /// <summary>Our own supplies with no invoice raised yet. Should be zero — the generator
        /// can produce these, so anything here is a run that has not happened.</summary>
        public int OutwardAwaitingDocument { get; set; }

        /// <summary>Their supplies with no invoice received yet. This is the queue: it cannot be
        /// generated, only chased, which is why it has to be visible.</summary>
        public int InwardAwaitingDocument { get; set; }

        public decimal InwardAwaitingAmount { get; set; }
    }

    public class ExchangeRegisterResult
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        public decimal TotalOutward { get; set; }
        public decimal TotalInward { get; set; }

        /// <summary>Outward + inward across every partner — the turnover a swap hides.</summary>
        public decimal TotalTurnover { get; set; }

        public int TotalAwaitingDocument { get; set; }
        public decimal TotalAwaitingAmount { get; set; }

        public List<ExchangeRegisterRow> Rows { get; set; } = new();
    }
}
