namespace FintechApp.Requests.TaxEngine
{
    /// <summary>Filter for the rule list.</summary>
    public class TaxRuleListRequest
    {
        /// <summary>SALE, PURCHASE, or empty for both.</summary>
        public string Direction { get; set; } = string.Empty;

        /// <summary>0 for every service.</summary>
        public int ServiceID { get; set; }

        /// <summary>True hides superseded and withdrawn rules. False is how you see history.</summary>
        public bool ActiveOnly { get; set; } = true;
    }

    /// <summary>Identifies one rule.</summary>
    public class TaxRuleIdRequest
    {
        public int TaxRuleMasterID { get; set; }
    }

    /// <summary>Withdraw a rule from a date.</summary>
    public class DeactivateTaxRuleRequest
    {
        public int TaxRuleMasterID { get; set; }
        public DateTime EffectiveTo { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Read or write one party's tax identity.</summary>
    public class PartyTaxProfileRequest
    {
        public int UserID { get; set; }
        public string GSTIN { get; set; } = string.Empty;
        public string RegistrationStatus { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
        public bool Is206ABFlagged { get; set; }

        /// <summary>Both legs of this party's wallet swaps must be invoiced. Setting it moves every
        /// credit to them out of ordinary top-up invoicing and into the exchange register.</summary>
        public bool IsExchangePartner { get; set; }

        public string ExchangeAgreementRef { get; set; } = string.Empty;

        // Section 197 lower-deduction certificate, if one is in force.
        public string LowerDeductionCertNo { get; set; } = string.Empty;
        public decimal LDCRate { get; set; }
        public DateTime? LDCValidFrom { get; set; }
        public DateTime? LDCValidTill { get; set; }
        public decimal LDCAmountCap { get; set; }

        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>P2P or AGENT for one party on one service — the Bharti Cellular switch.</summary>
    public class PartyRelationshipRequest
    {
        public int UserID { get; set; }

        /// <summary>0 = every service.</summary>
        public int ServiceID { get; set; }

        /// <summary>P2P (a discount, no 194H) or AGENT (commission, 194H applies).</summary>
        public string RelationshipType { get; set; } = string.Empty;

        /// <summary>The agreement this rests on. Pure-agent and principal-to-principal treatment
        /// both turn on what the contract says; a row with nothing behind it is an assertion.</summary>
        public string AgreementRef { get; set; } = string.Empty;

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>A date range, for the queries that take one.</summary>
    public class PeriodRequest
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        /// <summary>For the alert queries: how far ahead to look.</summary>
        public int DaysAhead { get; set; } = 30;

        /// <summary>For the queue: how many to attempt in one pass.</summary>
        public int MaxDocuments { get; set; } = 100;
    }

    /// <summary>Filter for the document list.</summary>
    public class InvoiceListRequest
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        /// <summary>SALE, PURCHASE, or empty for both.</summary>
        public string Direction { get; set; } = string.Empty;

        /// <summary>TaxInvoice / CreditNote / BillOfSupply / SelfInvoice, or empty for all.</summary>
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>0 for every party.</summary>
        public int PartyUserID { get; set; }
    }

    /// <summary>Identifies one document.</summary>
    public class DocumentIdRequest
    {
        public int TaxInvoiceHeaderID { get; set; }
    }
}
