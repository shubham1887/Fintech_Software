using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// How much TDS comes off a commission, and whose.
    ///
    /// TDS existed before this as a single switch on the commission tax profile: on or off per
    /// service / provider / slab, at the section's statutory rate. That cannot express what the
    /// business actually does — "0.01% on this service, for every retailer, except this one
    /// distributor who is on a flat ₹2" — so the answer lived in nobody's hands.
    ///
    /// One row per decision, matched MOST SPECIFIC FIRST:
    ///   USER_SERVICE → USER → USERTYPE_SERVICE → USERTYPE → SERVICE → GLOBAL
    ///
    /// Nothing here deducts anything on its own. The Key Setting master switch
    /// (<see cref="StaticModels.SettingKeys.TDSEnabled"/>) has to be on first, because turning
    /// TDS on withholds real money from real people.
    /// </summary>
    public class TdsRule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TdsRuleID { get; set; }

        /// <summary>See <see cref="StaticModels.TdsScope"/>.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Scope { get; set; } = StaticModels.TdsScope.Global;

        /// <summary>The party this rule is for. 0 = any, and required by the USER scopes.</summary>
        public int UserID { get; set; }

        /// <summary>The role this rule is for (UserTypeID). 0 = any, required by the USERTYPE scopes.</summary>
        public int UserTypeID { get; set; }

        /// <summary>The service. 0 = any, required by the …_SERVICE and SERVICE scopes.</summary>
        public int ServiceID { get; set; }

        /// <summary>Narrows a rule to one operator within the service. 0 = any.</summary>
        public int OperatorID { get; set; }

        /// <summary>PERCENT or FIX — see <see cref="StaticModels.TdsChargeType"/>.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ChargeType { get; set; } = StaticModels.TdsChargeType.Percent;

        /// <summary>
        /// The figure. Under PERCENT it is a percentage of the base (0.01 means 0.01%, which is
        /// what the brief asks for — not 1%); under FIX it is rupees per transaction.
        /// Four decimals, because a rate like 0.0125% is a real thing in this trade.
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal ChargeValue { get; set; }

        /// <summary>COMMISSION_NET (default, GST backed out) or COMMISSION_GROSS.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ApplyOn { get; set; } = StaticModels.TdsApplyOn.CommissionNet;

        /// <summary>
        /// Do not deduct where the commission is below this. 0 = always deduct. It exists because
        /// a flat ₹2 against a ₹1.50 commission would hand the earner a negative payout.
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MinCommission { get; set; }

        /// <summary>
        /// Cap per transaction. 0 = no cap. Guards a percentage rule against one freak transaction.
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MaxTDSPerTransaction { get; set; }

        /// <summary>
        /// The section the deduction is made under — 194H for commission, 194O for an e-commerce
        /// operator. Blank uses 194H, which is what this trade's commissions are.
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string SectionCode { get; set; } = string.Empty;

        /// <summary>
        /// Apply the section's higher no-PAN rate when the party has no PAN on record. Only
        /// meaningful for PERCENT: a flat figure has no statutory higher twin.
        /// </summary>
        public bool ApplyNoPANRate { get; set; } = true;

        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; } = new DateTime(2000, 1, 1);

        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>Higher wins among rules of the same scope. 0 lets the scope decide, so nobody
        /// has to reason about numbers to get the obvious behaviour.</summary>
        public int Priority { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Remark { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int CreatedByUserID { get; set; }
    }

    /// <summary>
    /// What was done with one party's withheld TDS for one month: deposited with the government,
    /// or given back as wallet balance.
    ///
    /// One row per (period, party) — a month is settled once per person, and the unique index says
    /// so, because settling twice either double-pays the government or double-credits a wallet.
    /// </summary>
    public class TdsSettlement : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TdsSettlementID { get; set; }

        /// <summary>yyyyMM — the month the deductions belong to.</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        public int PartyUserID { get; set; }

        /// <summary>DEPOSITED or WALLET_SETTLED — see <see cref="StaticModels.TdsSettlementMode"/>.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Mode { get; set; } = string.Empty;

        /// <summary>The total withheld in the period, as it stood when this was settled.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TDSAmount { get; set; }

        /// <summary>How many deductions that total is made of — what a certificate would list.</summary>
        public int DeductionCount { get; set; }

        /// <summary>The challan / BSR / UTR reference for a deposit, or the wallet posting id.</summary>
        [Column(TypeName = "varchar(100)")]
        public string Reference { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime? ReferenceDate { get; set; }

        /// <summary>The wallet the money went back into, for a WALLET_SETTLED row. 0 otherwise.</summary>
        public int WalletID { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Remark { get; set; } = string.Empty;

        public DateTime SettledOn { get; set; }

        public int SettledByUserID { get; set; }
    }
}
