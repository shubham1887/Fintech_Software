namespace FintechApp.Responses.Tds
{
    /// <summary>One configured rule, as the screen shows it.</summary>
    public class TdsRuleRow
    {
        public int TdsRuleID { get; set; }

        public string Scope { get; set; } = string.Empty;

        /// <summary>The scope spelled out — "every Retailer on Money Transfer" — so the list reads
        /// as sentences rather than as a grid of ids.</summary>
        public string ScopeLabel { get; set; } = string.Empty;

        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;

        public string ChargeType { get; set; } = string.Empty;
        public decimal ChargeValue { get; set; }
        public string ApplyOn { get; set; } = string.Empty;

        public decimal MinCommission { get; set; }
        public decimal MaxTDSPerTransaction { get; set; }

        public string SectionCode { get; set; } = string.Empty;
        public bool ApplyNoPANRate { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public int Priority { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>What this rule does, in one sentence.</summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>The TDS Setting screen in one payload.</summary>
    public class TdsSettingView
    {
        /// <summary>The Key Setting master switch. Every rule below is dormant while this is off,
        /// and the screen says so rather than letting somebody configure into a void.</summary>
        public bool MasterSwitchOn { get; set; }

        public List<TdsRuleRow> Rules { get; set; } = new();

        /// <summary>The sections available, so the screen offers them rather than asking for a code.</summary>
        public List<TdsSectionOption> Sections { get; set; } = new();

        public string Note { get; set; } = string.Empty;
    }

    public class TdsSectionOption
    {
        public string SectionCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal NoPANRate { get; set; }
        public decimal ThresholdAnnual { get; set; }
    }

    /// <summary>Every step of one hypothetical deduction, in words.</summary>
    public class TdsPreviewResponse
    {
        public bool MasterSwitchOn { get; set; }
        public bool RuleFound { get; set; }

        public int TdsRuleID { get; set; }
        public string Scope { get; set; } = string.Empty;
        public string DecidedBy { get; set; } = string.Empty;

        public decimal CommissionAsQuoted { get; set; }
        public decimal GSTInCommission { get; set; }
        public decimal Base { get; set; }

        public string ChargeType { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal NetToParty { get; set; }

        public string SectionCode { get; set; } = string.Empty;
        public bool NoPANRateApplied { get; set; }

        public List<string> Steps { get; set; } = new();
        public string Warning { get; set; } = string.Empty;
    }

    /// <summary>One party's month.</summary>
    public class TdsMonthlyRow
    {
        public int PartyUserID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyCode { get; set; } = string.Empty;
        public string UserTypeName { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;

        /// <summary>Blank PAN is called out: without one the deduction is at the higher rate and
        /// no certificate can be issued, which is a chase-the-party task, not a tax question.</summary>
        public bool PANMissing { get; set; }

        public int DeductionCount { get; set; }
        public decimal CommissionBase { get; set; }
        public decimal TDSAmount { get; set; }

        /// <summary>What was recorded but never actually taken out of the wallet. Kept apart so
        /// nobody deposits money the platform does not hold.</summary>
        public decimal NotWithheld { get; set; }

        public List<string> Sections { get; set; } = new();

        /// <summary>DEPOSITED / WALLET_SETTLED / empty when nothing has been done yet.</summary>
        public string SettlementMode { get; set; } = string.Empty;
        public string SettlementReference { get; set; } = string.Empty;
        public DateTime? SettledOn { get; set; }
        public decimal SettledAmount { get; set; }

        /// <summary>True while this month's figure can still be acted on.</summary>
        public bool Pending { get; set; }
    }

    public class TdsMonthlyReport
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public bool MasterSwitchOn { get; set; }

        public List<TdsMonthlyRow> Rows { get; set; } = new();

        public int PartyCount { get; set; }
        public decimal TotalTDS { get; set; }
        public decimal TotalNotWithheld { get; set; }
        public decimal TotalDeposited { get; set; }
        public decimal TotalWalletSettled { get; set; }
        public decimal TotalPending { get; set; }

        public string Note { get; set; } = string.Empty;
    }

    /// <summary>One deduction behind a party's monthly figure.</summary>
    public class TdsDeductionDetailRow
    {
        public int TransactionMasterID { get; set; }
        public DateTime DateTime { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public decimal CommissionBase { get; set; }
        public string ChargeType { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
        public decimal TDSAmount { get; set; }
        public string SectionCode { get; set; } = string.Empty;
        public bool NoPANRateApplied { get; set; }
        public bool Withheld { get; set; }
        public string DecidedBy { get; set; } = string.Empty;
    }

    public class TdsPartyDetailResponse
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int PartyUserID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public decimal TotalTDS { get; set; }
        public List<TdsDeductionDetailRow> Rows { get; set; } = new();
    }
}
