namespace FintechApp.StaticModels
{
    /// <summary>The returns this system builds. Each is derived — nothing here is entered twice.</summary>
    public static class GstReturnType
    {
        /// <summary>Outward supplies, invoice by invoice.</summary>
        public const string Gstr1 = "GSTR1";

        /// <summary>The monthly summary and the tax actually payable.</summary>
        public const string Gstr3b = "GSTR3B";

        /// <summary>The annual return: twelve months consolidated, books against what was filed.</summary>
        public const string Gstr9 = "GSTR9";

        public static readonly string[] All = { Gstr1, Gstr3b, Gstr9 };
    }

    /// <summary>Where a saved return stands. Only one Filed snapshot per return per period is live.</summary>
    public static class GstFilingStatus
    {
        /// <summary>Generated and kept, but not declared as filed. Safe to regenerate.</summary>
        public const string Draft = "Draft";

        /// <summary>Declared filed on the portal. GSTR-9 reconciles against these.</summary>
        public const string Filed = "Filed";

        /// <summary>Replaced by a later generation of the same return and period.</summary>
        public const string Superseded = "Superseded";
    }

    /// <summary>
    /// The GSTR-1 tables. These are the portal's own names, kept verbatim so a figure on the screen
    /// can be read straight across to the return being filed.
    /// </summary>
    public static class Gstr1Table
    {
        /// <summary>4A/4B/4C — supplies to a registered person, invoice by invoice.</summary>
        public const string B2B = "B2B";

        /// <summary>5A — inter-state supplies to an unregistered person above the invoice-value
        /// limit, still reported invoice by invoice.</summary>
        public const string B2CL = "B2CL";

        /// <summary>7 — every other supply to an unregistered person, consolidated by place of
        /// supply and rate. No document numbers: that is the point of the table.</summary>
        public const string B2CS = "B2CS";

        /// <summary>9B — credit and debit notes against a registered party's document.</summary>
        public const string CDNR = "CDNR";

        /// <summary>9B — credit and debit notes against an unregistered B2CL document.</summary>
        public const string CDNUR = "CDNUR";

        /// <summary>8 — nil-rated, exempted and non-GST outward supplies.</summary>
        public const string EXEMP = "EXEMP";

        /// <summary>12 — the HSN/SAC summary of everything supplied.</summary>
        public const string HSN = "HSN";

        /// <summary>13 — the document series issued, with the numbers used and cancelled.</summary>
        public const string DOCS = "DOCS";
    }

    public static class GstReturnLimits
    {
        /// <summary>
        /// The invoice value above which an inter-state supply to an unregistered person is
        /// reported individually (B2CL) rather than consolidated into B2CS.
        ///
        /// This number has moved: it was Rs 2,50,000 for years and was reduced to Rs 1,00,000 by
        /// Notification 12/2024. It is a CONSTANT here and overridable per request, because a
        /// return for an older period must be built on the limit that applied THEN — rebuilding
        /// September 2024 on today's limit would move invoices between two tables and stop the
        /// return agreeing with the one already filed.
        /// </summary>
        public const decimal B2CLInvoiceValue = 100000m;

        /// <summary>The limit that applied before Notification 12/2024, for rebuilding an older
        /// period. The caller passes it; nothing here guesses a date cut-over.</summary>
        public const decimal B2CLInvoiceValueLegacy = 250000m;
    }
}
