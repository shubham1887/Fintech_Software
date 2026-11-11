namespace FintechApp.Interfaces.Services
{
    // The commissions resolved for one recharge-family transaction from the retailer's slab.
    // Values are SIGNED: a surcharge slab yields a negative value (the user pays extra),
    // a commission slab yields a positive value (the user earns).
    public class ResolvedCommission
    {
        public bool SlabFound { get; set; }

        /// <summary>
        /// Set when the matching band exists but may NOT be paid — today that means a delegated
        /// slab whose parent card was lowered past what this band hands out, which leaves the
        /// band marked INVALID until its owner corrects it.
        ///
        /// This is deliberately not expressed as "no slab found". Falling through to the upline's
        /// card would pay MORE than the owner now allows, which is the exact mistake invalidation
        /// exists to prevent, and silently paying zero would read to a retailer as a service that
        /// has stopped earning. The transaction services refuse the transaction and say why, so
        /// the one person who can fix it hears about it from the person it is costing.
        /// </summary>
        public string BlockedReason { get; set; } = string.Empty;

        public bool IsBlocked => !string.IsNullOrEmpty(BlockedReason);

        // What the transacting user earns/pays.
        public decimal RetailerComm { get; set; }

        // Paid only when the matching upline exists in the chain; otherwise 0 and the slab value
        // implicitly stays with admin (Case 2: Admin → Retailer direct).
        public decimal DistributorComm { get; set; }
        public decimal SuperDistributorComm { get; set; }

        public int DistributorUserID { get; set; }
        public int SuperDistributorUserID { get; set; }

        // ── Who PAYS each tier ───────────────────────────────────────────────────────────────
        // 0 means the platform operator (the admin ancestor), which is how every transaction on a
        // non-delegated install reads — and what the posting code did unconditionally before.
        //
        // A delegated card changes this and nothing else about the engine. When a retailer's rate
        // comes from a card a DISTRIBUTOR wrote, the distributor is the one who funds it: their
        // wallet is debited for the retailer's commission, while their own commission is credited
        // to them from the card ABOVE them. The difference between the two is their margin, and it
        // exists as two real postings rather than as a figure on a rate card.
        public int RetailerFunderID { get; set; }
        public int DistributorFunderID { get; set; }
        public int SuperDistributorFunderID { get; set; }

        /// <summary>
        /// What the platform operator pays of this transaction's chain commissions — the tiers no
        /// reseller priced. Signed, so a surcharge the operator collects reduces it.
        ///
        /// This, not the sum of all three tiers, is what the operator's profit is measured against:
        /// a retailer commission funded by a distributor never touches the operator's wallet, and
        /// counting it as a cost reported a loss that nobody made. On an install without delegation
        /// every funder is 0 and this is the same figure the old expression produced.
        /// </summary>
        public decimal AdminFundedCommission =>
            (RetailerFunderID == 0 ? RetailerComm : 0m)
            + (DistributorFunderID == 0 ? DistributorComm : 0m)
            + (SuperDistributorFunderID == 0 ? SuperDistributorComm : 0m);

        // ── Which row decided, and what it says about tax ────────────────────────────────────
        // Carried out of the resolver because the row is where the GST decision now lives, and
        // finding it a second time would mean matching the same band twice — and risking a
        // different answer if the card was edited in between.
        public int SlabMasterID { get; set; }
        public int SlabDetailID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }

        /// <summary>INCLUSIVE / EXCLUSIVE / NONE, or DEFAULT to fall through to the profile.</summary>
        public string GSTType { get; set; } = StaticModels.ChargeGstType.Default;

        /// <summary>0 = ask the tax engine.</summary>
        public decimal GSTRatePercent { get; set; }

        /// <summary>FLAT or PERCENTAGE, canonical — the row's own column is free text.</summary>
        public string RetailerChargeType { get; set; } = string.Empty;
        public string DistributorChargeType { get; set; } = string.Empty;
        public string SuperDistributorChargeType { get; set; } = string.Empty;
    }

    // Resolves the slab commission (or surcharge) for a user + operator + amount.
    //
    // The transacting user's own rate comes from the card assigned to THEM (their mapping, else
    // the nearest upline's). Each upline's rate comes from the card assigned to THAT upline, which
    // is what makes a reseller's margin work: the distributor is paid ₹12 by whoever wrote their
    // card and pays the retailer ₹10 out of their own wallet, instead of admin paying ₹10 and ₹2
    // separately off one shared card. On an install where one admin card is assigned to everyone,
    // every lookup lands on that same card and the answer is identical to what it always was.
    public interface ISlabCommissionResolver
    {
        Task<ResolvedCommission> Resolve(int userID, int serviceID, int operatorID, decimal amount);
    }
}
