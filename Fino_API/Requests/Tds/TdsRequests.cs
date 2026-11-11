namespace FintechApp.Requests.Tds
{
    /// <summary>Create or update one TDS rule.</summary>
    public class SaveTdsRuleRequest
    {
        public int TdsRuleID { get; set; }

        /// <summary>USER_SERVICE / USER / USERTYPE_SERVICE / USERTYPE / SERVICE / GLOBAL.</summary>
        public string Scope { get; set; } = string.Empty;

        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        /// <summary>PERCENT or FIX.</summary>
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>A percentage (0.01 = 0.01%) or rupees per transaction.</summary>
        public decimal ChargeValue { get; set; }

        /// <summary>COMMISSION_NET (default) or COMMISSION_GROSS.</summary>
        public string ApplyOn { get; set; } = string.Empty;

        public decimal MinCommission { get; set; }
        public decimal MaxTDSPerTransaction { get; set; }

        public string SectionCode { get; set; } = string.Empty;
        public bool ApplyNoPANRate { get; set; } = true;

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public int Priority { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        /// <summary>Set from the authenticated identity, never from the body.</summary>
        public int CreatedByUserID { get; set; }
    }

    public class TdsRuleIdRequest
    {
        public int TdsRuleID { get; set; }
    }

    /// <summary>Try a commission through the rules without deducting anything.</summary>
    public class TdsPreviewRequest
    {
        /// <summary>The party who would earn the commission. Required — the rules are about people.</summary>
        public int UserID { get; set; }

        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        /// <summary>The commission as quoted, GST included where it is inclusive.</summary>
        public decimal Commission { get; set; }

        /// <summary>
        /// The GST inside that commission, when it is known. Left at 0 the preview backs it out
        /// using the commission tax profile, which is what a real settlement does.
        /// </summary>
        public decimal CommissionGST { get; set; }

        public DateTime? AsOf { get; set; }
    }

    public class TdsMonthlyReportRequest
    {
        /// <summary>yyyyMM. Empty means the current month.</summary>
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>0 = every party.</summary>
        public int PartyUserID { get; set; }

        /// <summary>Only the parties not yet settled, for the desk working through a month.</summary>
        public bool PendingOnly { get; set; }
    }

    /// <summary>Record that a party's month of TDS was paid to the government.</summary>
    public class TdsDepositRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int PartyUserID { get; set; }

        /// <summary>The challan / BSR / UTR reference. Required: a deposit nobody can trace is not
        /// a deposit, and this is the number a TDS certificate is issued against.</summary>
        public string Reference { get; set; } = string.Empty;

        public DateTime? ReferenceDate { get; set; }
        public string Remark { get; set; } = string.Empty;

        public int SettledByUserID { get; set; }
    }

    /// <summary>Give a party's withheld TDS back as wallet balance instead of depositing it.</summary>
    public class TdsWalletSettleRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int PartyUserID { get; set; }

        /// <summary>The wallet to credit. 0 uses the party's main wallet.</summary>
        public int WalletID { get; set; }

        /// <summary>Why it is being returned rather than deposited. Required — this is money going
        /// back out, and in six months somebody will ask on what grounds.</summary>
        public string Remark { get; set; } = string.Empty;

        public int SettledByUserID { get; set; }
    }
}
