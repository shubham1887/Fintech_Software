namespace FintechApp.Requests.CommissionTax
{
    /// <summary>Create or update one commission tax profile.</summary>
    public class SaveCommissionTaxProfileRequest
    {
        public int CommissionTaxProfileID { get; set; }

        /// <summary>GLOBAL / SERVICE / PROVIDER / SLAB.</summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>ServiceID, ProviderID or SlabMasterID to match the scope. 0 for GLOBAL.</summary>
        public int ScopeID { get; set; }

        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        /// <summary>INCLUSIVE / EXCLUSIVE / NONE.</summary>
        public string TaxMode { get; set; } = string.Empty;

        /// <summary>COMMISSION / GROSS.</summary>
        public string TaxBase { get; set; } = string.Empty;

        /// <summary>0 asks the tax engine for the rate as of the transaction's own date.</summary>
        public decimal GSTRatePercent { get; set; }

        public decimal PerTransactionCharge { get; set; }
        public string PerTransactionChargeTaxMode { get; set; } = string.Empty;

        public bool TDSEnabled { get; set; }
        public string TDSSectionCode { get; set; } = string.Empty;
        public decimal TDSRatePercent { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int Priority { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Try a commission through the configured treatment and see every step.</summary>
    public class CommissionTaxPreviewRequest
    {
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int SlabMasterID { get; set; }

        /// <summary>The transaction amount, for a GROSS-based treatment.</summary>
        public decimal Amount { get; set; }

        /// <summary>The commission as the slab or the provider contract quotes it.</summary>
        public decimal Commission { get; set; }

        /// <summary>The party, so the TDS section and the no-PAN rate can be resolved. 0 skips it.</summary>
        public int PartyUserID { get; set; }

        /// <summary>Resolve as of this date. Default is today.</summary>
        public DateTime? AsOf { get; set; }
    }

    public class CommissionTaxProfileQuery
    {
        /// <summary>GLOBAL / SERVICE / PROVIDER / SLAB, or blank for all.</summary>
        public string Scope { get; set; } = string.Empty;
    }

    public class CommissionTaxProfileIdRequest
    {
        public int CommissionTaxProfileID { get; set; }
    }
}
