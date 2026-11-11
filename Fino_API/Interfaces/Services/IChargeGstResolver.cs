namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// One charge, split into what it is worth and what of it is tax.
    ///
    /// Everything here is SIGNED the way the rest of the commission pipeline is signed: positive =
    /// earned, negative = paid away (a surcharge the user pays, or a provider charge such as DMT).
    /// GST and base carry the sign of the charge, so sums stay meaningful without the caller
    /// knowing which rail produced the figure.
    /// </summary>
    public class ChargeGstBreakdown
    {
        /// <summary>The figure the rate card quotes, before anything is decided about tax.</summary>
        public decimal ChargeAsQuoted { get; set; }

        /// <summary>FLAT or PERCENTAGE — see <see cref="StaticModels.ChargeBasis"/>.</summary>
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>INCLUSIVE / EXCLUSIVE / NONE. Never DEFAULT: by the time a breakdown exists the
        /// fall-through has already happened and an actual decision has been made.</summary>
        public string GSTType { get; set; } = string.Empty;

        public decimal GSTRatePercent { get; set; }

        /// <summary>The tax inside the charge (INCLUSIVE) or added to it (EXCLUSIVE).</summary>
        public decimal GSTAmount { get; set; }

        /// <summary>The charge without tax. This is the income, and what TDS is computed on.</summary>
        public decimal BaseCharge { get; set; }

        /// <summary>Base + GST: what the leg actually settles at. Equals the quoted figure when the
        /// band was INCLUSIVE, and quoted + GST when it was EXCLUSIVE.</summary>
        public decimal TotalCharge { get; set; }

        /// <summary>Which layer decided, in words — the band, which profile, or the fallback. Kept
        /// so a screen and a support answer can say WHY, rather than restating the number.</summary>
        public string DecidedBy { get; set; } = string.Empty;

        /// <summary>Set when the rate or the treatment had to be assumed. Carried, never hidden.</summary>
        public string Warning { get; set; } = string.Empty;

        /// <summary>True when a rate card row was actually matched for this transaction.</summary>
        public bool BandMatched { get; set; }

        /// <summary>The matched row, for the screens that want to link to it. 0 = none.</summary>
        public int BandID { get; set; }

        public decimal BandFromAmount { get; set; }
        public decimal BandToAmount { get; set; }
    }

    /// <summary>
    /// The GST on a charge, resolved per transaction from configuration only — no formula anywhere
    /// else in the product, and no literal anywhere at all.
    ///
    /// Four layers, most specific first, so that an install can be as coarse or as detailed as its
    /// contracts actually are:
    ///   1. the matched rate card ROW (a provider commission band, a slab band) — per band;
    ///   2. the commission tax profile, matched SLAB → PROVIDER → SERVICE → GLOBAL;
    ///   3. the tax engine's effective-dated rate for the service, when neither pins a number;
    ///   4. the stated built-in fallback, which says so in <see cref="ChargeGstBreakdown.Warning"/>.
    /// </summary>
    public interface IChargeGstResolver
    {
        /// <summary>
        /// What the provider charge or commission on this transaction is worth net of GST. The
        /// quoted figure is passed in because the transaction already carries it: the gateway
        /// resolved it at dispatch, and re-resolving a card that may since have been edited would
        /// restate history.
        /// </summary>
        Task<ChargeGstBreakdown> ForProviderCharge(
            int providerID, int serviceID, int operatorID, decimal amount, decimal quotedCharge, DateTime asOf);


        /// <summary>
        /// The same for the downline side, all three tiers off ONE slab match. The tiers share the
        /// band's GST decision because a slab band is one commercial decision, and matching the
        /// band once means the three tiers cannot disagree about which row priced them.
        ///
        /// The quoted figures are passed in because a settled transaction already carries them;
        /// re-resolving a card that may since have been edited would restate history.
        /// </summary>
        Task<SlabSideGst> ForSlabSide(
            int userID, int serviceID, int operatorID, decimal amount,
            decimal retailerQuoted, decimal distributorQuoted, decimal superDistributorQuoted, DateTime asOf);

        /// <summary>
        /// Both sides of one hypothetical transaction, for the preview screen. Nothing is written,
        /// and the figures come from the card as it stands right now.
        /// </summary>
        Task<ChargeGstPreview> Preview(
            int userID, int providerID, int serviceID, int operatorID, decimal amount, DateTime asOf);
    }

    /// <summary>The three downline tiers, split under the one slab band that priced them.</summary>
    public class SlabSideGst
    {
        public ChargeGstBreakdown Retailer { get; set; } = new();
        public ChargeGstBreakdown Distributor { get; set; } = new();
        public ChargeGstBreakdown SuperDistributor { get; set; } = new();

        public int SlabMasterID { get; set; }
        public int SlabDetailID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool BandMatched { get; set; }
    }

    /// <summary>What a transaction of this size WOULD cost and earn, both sides, without settling one.</summary>
    public class ChargeGstPreview
    {
        public decimal Amount { get; set; }

        public ChargeGstBreakdown Provider { get; set; } = new();
        public ChargeGstBreakdown Retailer { get; set; } = new();
        public ChargeGstBreakdown Distributor { get; set; } = new();
        public ChargeGstBreakdown SuperDistributor { get; set; } = new();

        /// <summary>The slab the user resolved onto, by name, and the band that matched.</summary>
        public string SlabName { get; set; } = string.Empty;
        public int SlabMasterID { get; set; }
        public int SlabDetailID { get; set; }

        /// <summary>
        /// What the house keeps: the provider leg net of its GST, less every downline charge net of
        /// theirs. Net against net — mixing a GST-net cost with a GST-gross payout is exactly how
        /// DMT profit once read four times what it was.
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>Every step, in words, for an admin who has to see WHY before a month of
        /// transactions is booked on it.</summary>
        public List<string> Notes { get; set; } = new();
    }
}
