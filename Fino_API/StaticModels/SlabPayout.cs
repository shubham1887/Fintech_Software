using FintechApp.Entities;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// What a commission band pays out, as a function of the transaction amount.
    ///
    /// Every tier on a band is either a flat figure or a percentage of the amount, and either a
    /// commission (money to the user) or a surcharge (money from them). Added up, a band's payout
    /// is therefore a straight line in the amount: <c>Flat + Percent% × amount</c>. Writing it
    /// down that way is what makes the delegation ceiling checkable at all once a reseller is
    /// allowed to choose its own bands and its own types — there is no longer a parent ROW to
    /// compare a child row against, only a parent CARD that pays some amount at every point, and
    /// the question becomes "is the child's line under the parent's line everywhere in this band".
    ///
    /// Signed throughout: positive = paid out, negative = charged. So "child must not exceed
    /// parent" reads correctly in both directions at once — you may not pay a retailer more than
    /// you were paid, and you may not charge them less than you were charged.
    /// </summary>
    public readonly record struct PayoutCurve(decimal Flat, decimal Percent)
    {
        public static readonly PayoutCurve Zero = new(0m, 0m);

        /// <summary>What this pays out at one transaction amount.</summary>
        public decimal At(decimal amount) => Flat + amount * Percent / 100m;

        public static PayoutCurve operator +(PayoutCurve a, PayoutCurve b)
            => new(a.Flat + b.Flat, a.Percent + b.Percent);

        public static PayoutCurve operator -(PayoutCurve a, PayoutCurve b)
            => new(a.Flat - b.Flat, a.Percent - b.Percent);

        public bool IsZero => Flat == 0m && Percent == 0m;

        /// <summary>
        /// True when this can be written back into ONE slab column — a column holds a single
        /// figure and a single type, so a curve with both a flat and a percentage part cannot be
        /// stored. That happens when a child sets a flat rate against a percentage parent (or the
        /// reverse), and the owner's leftover margin ends up being part of each.
        /// </summary>
        public bool IsStorableInOneColumn => Flat == 0m || Percent == 0m;
    }

    /// <summary>
    /// Reading a commission band as a <see cref="PayoutCurve"/>, and the ceiling check that keeps
    /// a delegated card inside the card it was cut from.
    /// </summary>
    public static class SlabPayout
    {
        /// <summary>
        /// Rounding tolerance. Commission is decimal(18,5) and a percentage card is typed to two
        /// or three places, so a total landing a hundredth of a paisa over its parent is
        /// arithmetic rather than an attempt to overpay.
        /// </summary>
        public const decimal Tolerance = 0.00001m;

        /// <summary>The four tier columns, in the order a card is read.</summary>
        public static readonly string[] Tiers =
        {
            SlabAuditTier.Retailer, SlabAuditTier.Distributor,
            SlabAuditTier.SuperDistributor, SlabAuditTier.Client,
        };

        public static decimal CommOf(SlabDetail row, string tier) => tier switch
        {
            SlabAuditTier.Retailer => row.RetailerComm,
            SlabAuditTier.Distributor => row.DistributorComm,
            SlabAuditTier.SuperDistributor => row.SuperDistributorComm,
            _ => row.ClientComm,
        };

        public static string TypeOf(SlabDetail row, string tier) => tier switch
        {
            SlabAuditTier.Retailer => row.RetailerCommType,
            SlabAuditTier.Distributor => row.DistributorCommType,
            SlabAuditTier.SuperDistributor => row.SuperDistributorCommType,
            _ => row.ClientCommType,
        };

        public static string CategoryOf(SlabDetail row, string tier) => tier switch
        {
            SlabAuditTier.Retailer => row.RetailerCommissionCategory,
            SlabAuditTier.Distributor => row.DistributorCommissionCategory,
            SlabAuditTier.SuperDistributor => row.SuperDistributorCommissionCategory,
            _ => row.ClientCommissionCategory,
        };

        /// <summary>
        /// A tier's figure as a SIGNED payout — negative for a surcharge. Mirrors how
        /// <c>SlabCommissionResolver.Normalize</c> decides a surcharge (either the category says
        /// so, or the stored value is already negative), so the limit enforced here and the money
        /// the engine actually moves are read off one rule.
        /// </summary>
        public static decimal Signed(decimal comm, string? category) =>
            (comm < 0 || Services.SlabCommissionResolver.IsSurchargeCategory(category))
                ? -System.Math.Abs(comm) : System.Math.Abs(comm);

        /// <summary>One tier of one band, as a curve.</summary>
        public static PayoutCurve CurveOf(SlabDetail row, string tier)
        {
            decimal signed = Signed(CommOf(row, tier), CategoryOf(row, tier));
            return Services.SlabCommissionResolver.IsPercentType(TypeOf(row, tier))
                ? new PayoutCurve(0m, signed)
                : new PayoutCurve(signed, 0m);
        }

        /// <summary>Everything a band pays out, across every tier, as one curve.</summary>
        public static PayoutCurve TotalCurve(SlabDetail row)
        {
            var total = PayoutCurve.Zero;
            foreach (var tier in Tiers) total += CurveOf(row, tier);
            return total;
        }

        /// <summary>Everything a band pays out EXCEPT one tier — the owner's own margin column.</summary>
        public static PayoutCurve TotalCurveExcept(SlabDetail row, string? exceptTier)
        {
            var total = PayoutCurve.Zero;
            foreach (var tier in Tiers)
                if (tier != exceptTier) total += CurveOf(row, tier);
            return total;
        }

        /// <summary>
        /// The parent bands that could govern <paramref name="child"/>'s service and operator,
        /// most specific first — the same precedence the commission engine's own band match uses
        /// (exact operator before wildcard, exact service before wildcard, then the narrowest
        /// range). A ServiceID or OperatorID of 0 on a parent row means "any", which is the legacy
        /// wildcard the engine already honours.
        /// </summary>
        public static List<SlabDetail> CandidateParents(IEnumerable<SlabDetail> parentBands, SlabDetail child)
            => parentBands
                .Where(r => r.IsActive && !r.IsDeleted
                            && (r.OperatorID == child.OperatorID || r.OperatorID == 0)
                            && (r.ServiceID == child.ServiceID || r.ServiceID == 0))
                .OrderBy(r => r.OperatorID == child.OperatorID ? 0 : 1)
                .ThenBy(r => r.ServiceID == child.ServiceID ? 0 : 1)
                .ThenBy(r => r.ToAmount - r.FromAmount)
                .ThenBy(r => r.SlabDetailID)
                .ToList();

        /// <summary>
        /// The one parent band that governs a given amount, by the engine's own precedence.
        /// Null when the parent card does not price that amount at all.
        /// </summary>
        public static SlabDetail? ParentAt(List<SlabDetail> candidates, decimal amount)
            => candidates.FirstOrDefault(r => amount >= r.FromAmount && amount <= r.ToAmount);

        /// <summary>
        /// What one band hands the owner for themselves — the ONE figure a reseller gets to spend.
        ///
        /// This is the whole economics of delegation in one line. The card above a distributor
        /// carries a figure in the DISTRIBUTOR column: that, and only that, is what the platform
        /// pays them per transaction. It is not the band's total — the other columns on that card
        /// are what the card's owner pays to the tiers it serves directly, and they are not the
        /// distributor's to re-spend.
        ///
        /// So an operator who wants a distributor to price its own retailers puts the whole net
        /// amount on the distributor's line (₹12, rather than the old ₹10 retailer + ₹2
        /// distributor split) and the distributor divides it: ₹10 down to the retailer, ₹2 kept.
        /// <paramref name="ownerTier"/> null means an admin card, which answers to nobody.
        /// </summary>
        public static PayoutCurve AllowanceCurve(SlabDetail parentBand, string? ownerTier)
            => ownerTier == null ? TotalCurve(parentBand) : CurveOf(parentBand, ownerTier);

        /// <summary>
        /// What one band costs its owner — every tier they hand out, which is every column except
        /// their own. Their own column is not theirs to fund: the engine pays an owner from the
        /// card ABOVE them, never from the card they wrote, so a figure sitting there would be
        /// money nobody pays. Delegation writes it as zero for exactly that reason.
        /// </summary>
        public static PayoutCurve OutlayCurve(SlabDetail row, string? ownerTier)
            => TotalCurveExcept(row, ownerTier);

        /// <summary>
        /// Whether <paramref name="child"/> stays inside what its owner was given, across the
        /// band's whole amount range, and the sentence to show the owner if it does not.
        ///
        /// The comparison is "what this card hands out" against "what the card above hands ME"
        /// (<see cref="AllowanceCurve"/>) — not against the parent band's total. A reseller
        /// spends its own rate; everything it does not spend it keeps, so no third number needs
        /// storing and the books cannot be made to disagree.
        ///
        /// Both sides are straight lines in the amount within any one parent band, so comparing
        /// them at the endpoints of each overlapping stretch is not a sample — it is exact. The
        /// breakpoints are the child band's own ends plus every parent band boundary that falls
        /// inside it, which is what makes a child band that spans a rate step get caught at the
        /// step rather than only at its edges.
        /// </summary>
        public static string ValidateAgainstParent(
            SlabDetail child, List<SlabDetail> candidates, decimal fromAmount, decimal toAmount,
            string? ownerTier)
        {
            if (candidates.Count == 0)
                return "Your parent's slab prices nothing for this service and operator, so there is no commission for you to share out.";

            var breakpoints = new SortedSet<decimal> { fromAmount, toAmount };
            foreach (var p in candidates)
            {
                if (p.FromAmount > fromAmount && p.FromAmount <= toAmount) breakpoints.Add(p.FromAmount);
                if (p.ToAmount >= fromAmount && p.ToAmount < toAmount) breakpoints.Add(p.ToAmount);
            }

            var childCurve = OutlayCurve(child, ownerTier);
            foreach (decimal amount in breakpoints)
            {
                var parent = ParentAt(candidates, amount);
                if (parent == null)
                    return $"Your parent's slab does not cover ₹{amount:0.##} for this operator, so you cannot price it.";

                decimal mine = childCurve.At(amount);
                decimal theirs = AllowanceCurve(parent, ownerTier).At(amount);
                if (mine > theirs + Tolerance)
                    return $"At ₹{amount:0.##} this band gives away ₹{mine:0.##}, but your own commission for that amount is only ₹{theirs:0.##}. Keep what you hand out at or below it.";
            }

            return string.Empty;
        }

        /// <summary>
        /// The single parent band a child band must sit inside, or null with a reason.
        ///
        /// A child may SPLIT one of its parent's bands into several of its own — that is the
        /// flexibility the delegation model is for — but it may not straddle two parent bands.
        /// The ceiling check would catch an overpayment either way (it is evaluated at every rate
        /// step inside the band), so what this rule really protects is the tax decision: GST
        /// treatment is inherited from the parent band and a straddling child band would have two
        /// parents to inherit it from.
        /// </summary>
        public static (SlabDetail? band, string failure) EnclosingParent(
            List<SlabDetail> candidates, decimal fromAmount, decimal toAmount)
        {
            var atFrom = ParentAt(candidates, fromAmount);
            var atTo = ParentAt(candidates, toAmount);

            if (atFrom == null || atTo == null)
                return (null, $"Your parent's slab does not price the whole ₹{fromAmount:0.##} – ₹{toAmount:0.##} range for this operator.");

            if (atFrom.SlabDetailID != atTo.SlabDetailID)
                return (null, $"This range crosses two of your parent's bands (it changes rate at ₹{atTo.FromAmount:0.##}). Split it into two bands so each one sits inside a single parent band.");

            return (atFrom, string.Empty);
        }
    }
}
