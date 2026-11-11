namespace FintechApp.Requests.Phase3
{
    /// <summary>One line of a captured purchase. Tax is taken as the supplier billed it, never
    /// re-resolved: a purchase invoice is their document, and the credit we may claim is the tax
    /// that is actually on it, to the paisa.</summary>
    public class PurchaseLineInput
    {
        public int ServiceID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SACCode { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal TaxableValue { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessAmount { get; set; }

        /// <summary>Whether the credit on this line may be claimed. A blocked credit (Section
        /// 17(5)) or a composition supplier means the tax is a cost, not a credit.</summary>
        public bool ITCEligible { get; set; } = true;

        public bool ReverseCharge { get; set; }
    }

    /// <summary>Record a supplier's invoice as a purchase document. The supplier is the party; the
    /// number is theirs, not one we allocate.</summary>
    public class PurchaseCaptureRequest
    {
        /// <summary>The supplier as a party (PartyTaxProfile is keyed on this). 0 if not set up as
        /// a party, in which case the GSTIN / name / state below identify them on the document.</summary>
        public int SupplierUserID { get; set; }
        public string SupplierGSTIN { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierStateCode { get; set; } = string.Empty;

        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }

        /// <summary>When the provider wallet was actually credited, for the three-way check. The
        /// invoice date and the wallet credit date differ (both suppliers dated 30-Sep, took their
        /// IRN 5-Oct), and which governs the period is the CA's call — so both are stored.</summary>
        public DateTime? WalletCreditDate { get; set; }

        /// <summary>The provider and wallet this purchase funded, for the three-way reconciliation.
        /// 0 when the purchase is not a provider wallet top-up (e.g. a service bill).</summary>
        public int ProviderID { get; set; }
        public int WalletID { get; set; }

        public string Remark { get; set; } = string.Empty;
        public List<PurchaseLineInput> Lines { get; set; } = new();

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Capture a purchase from a supplier's e-invoice JSON (the NIC/IRP schema). The same
    /// shape this platform builds for its own documents, parsed back the other way.</summary>
    public class EInvoiceImportRequest
    {
        public string Json { get; set; } = string.Empty;
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>Optional three-way link, as on a manual capture.</summary>
        public int ProviderID { get; set; }
        public int WalletID { get; set; }
        public DateTime? WalletCreditDate { get; set; }

        /// <summary>Map every item's SAC to one ServiceID, when the import cannot tell which service
        /// a line is. 0 leaves the line's ServiceID unset.</summary>
        public int DefaultServiceID { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Import a GSTR-2B snapshot for one period from the portal JSON.</summary>
    public class Gstr2bImportRequest
    {
        public string Json { get; set; } = string.Empty;
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>yyyyMM. If blank, the 2B's own return period (rtnprd, MMYYYY) is used.</summary>
        public string PeriodKey { get; set; } = string.Empty;

        public int ImportedByUserID { get; set; }
    }

    /// <summary>Run or commit a purchase-vs-2B reconciliation for a period.</summary>
    public class ReconRequest
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>Tax-head tolerance in rupees. 0 falls back to the house default (0.05).</summary>
        public decimal Tolerance { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Build (or rebuild) the ITC register for a period from the captured purchases.</summary>
    public class ItcBuildRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    public class ItcClaimRequest
    {
        public List<int> EntryIDs { get; set; } = new();
        /// <summary>The return period the credit is claimed in (yyyyMM). Defaults to the entry's own.</summary>
        public string ClaimPeriod { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    public class ItcReverseRequest
    {
        public int EntryID { get; set; }
        /// <summary>Rule37 / Section16_4 / Manual.</summary>
        public string Reason { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    public class ItcMarkPaidRequest
    {
        public int EntryID { get; set; }
        public DateTime? PaidOn { get; set; }
    }

    /// <summary>A date range for the alert and register reads.</summary>
    public class ItcQueryRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int DaysAhead { get; set; } = 30;
    }
}
