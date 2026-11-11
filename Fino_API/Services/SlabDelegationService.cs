using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SlabDelegation;
using FintechApp.Responses.SlabDelegation;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Hierarchy-based commission slab delegation.
    ///
    /// ── The invariant ──────────────────────────────────────────────────────────────────────
    /// Nobody hands out more than they hold. A reseller is credited ONE figure per transaction —
    /// whatever its own line says on the card above it — and its wallet is debited for whatever it
    /// passes down. So the rule each card is held to is simply: what this card gives away, at
    /// every amount, is no more than what its owner is paid at that amount. Not the parent band's
    /// total: the parent's other columns are the parent's own costs, not the reseller's income.
    ///
    /// ── Where the owner's margin lives ─────────────────────────────────────────────────────
    /// Nowhere. A distributor whose own card says ₹12 and who writes ₹10 onto a retailer card is
    /// credited ₹12 by the platform and debited ₹10 for the retailer: the ₹2 is the gap between
    /// two real postings, not a figure anyone typed or stored. That is why the owner's own column
    /// on a card they wrote is held at zero — a figure there would be money nobody funds, and the
    /// engine reads an owner's rate off the card ABOVE them, never off the one they wrote.
    ///
    /// The practical consequence for whoever sets the platform up: put a reseller's whole net
    /// commission on the reseller's own line. ₹12 on the distributor line, and the distributor
    /// decides the retailer's ₹10 — not the old ₹10 retailer + ₹2 distributor split, which under
    /// this model means the distributor only has ₹2 to spend.
    ///
    /// ── Signed values ───────────────────────────────────────────────────────────────────────
    /// A band's tier can be a commission (money to the user) or a surcharge (money from them).
    /// Comparisons here are on the SIGNED payout — surcharge negative, commission positive — which
    /// makes "a child may not exceed its parent" read correctly in both directions at once: you may
    /// not pay a retailer more than you were paid, and you may not charge them less than you were
    /// charged. Same expression, both cases. <see cref="Signed"/>.
    /// </summary>
    public class SlabDelegationService : ISlabDelegationService
    {
        private readonly ISlabMasterRepository _slabMasterRepo;
        private readonly ISlabDetailRepository _slabDetailRepo;
        private readonly IUserSlabMappingRepository _mappingRepo;
        private readonly ISlabCommissionAuditRepository _auditRepo;
        private readonly IUserRepository _userRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly Logger _logger;

        /// <summary>Matches SlabCommissionResolver.MaxChainWalk — the same tree, walked the same way.</summary>
        private const int MaxChainWalk = 8;

        /// <summary>
        /// Rounding tolerance for the row-total check. Commission values are decimal(18,5) and a
        /// percentage card is typed to two or three places, so a total that lands a hundredth of a
        /// paisa over its parent is arithmetic, not an attempt to overpay.
        /// </summary>
        private const decimal Tolerance = 0.00001m;

        public SlabDelegationService(
            ISlabMasterRepository slabMasterRepo,
            ISlabDetailRepository slabDetailRepo,
            IUserSlabMappingRepository mappingRepo,
            ISlabCommissionAuditRepository auditRepo,
            IUserRepository userRepo,
            IServiceRepository serviceRepo,
            IOperatorRepository operatorRepo)
        {
            _slabMasterRepo = slabMasterRepo;
            _slabDetailRepo = slabDetailRepo;
            _mappingRepo = mappingRepo;
            _auditRepo = auditRepo;
            _userRepo = userRepo;
            _serviceRepo = serviceRepo;
            _operatorRepo = operatorRepo;
            _logger = new Logger();
        }

        private static bool IsAdminTier(int userTypeID) =>
            userTypeID == UserTypeCode.Admin || userTypeID == UserTypeCode.SupportUser;
        // ── Tier vocabulary ─────────────────────────────────────────────────────────────────
        // The column names, how to read a band as a payout curve and how to write a figure back
        // all live in SlabPayout now, because the ceiling check needs them too and two copies of
        // "what does a surcharge mean" is exactly the kind of drift this feature cannot afford.
        private static readonly string[] Tiers = SlabPayout.Tiers;

        private static decimal CommOf(SlabDetail row, string tier) => SlabPayout.CommOf(row, tier);
        private static string TypeOf(SlabDetail row, string tier) => SlabPayout.TypeOf(row, tier);
        private static string CategoryOf(SlabDetail row, string tier) => SlabPayout.CategoryOf(row, tier);

        /// <summary>
        /// Write one tier's figure onto a row, keeping the "Real*" mirror in step. The engine reads
        /// the plain columns and several reports read the Real* ones; the slab editor has always
        /// written both to the same value, and a delegated card that wrote only one of them would
        /// show one number on the card and another on the Commission Structure report.
        /// </summary>
        private static void SetComm(SlabDetail row, string tier, decimal value)
        {
            switch (tier)
            {
                case SlabAuditTier.Retailer:
                    row.RetailerComm = value; row.RealRetailerComm = value; break;
                case SlabAuditTier.Distributor:
                    row.DistributorComm = value; row.RealDistributorComm = value; break;
                case SlabAuditTier.SuperDistributor:
                    row.SuperDistributorComm = value; row.RealSuperDistributorComm = value; break;
                default:
                    row.ClientComm = value; row.RealClientComm = value; break;
            }
        }

        /// <summary>Set a tier's commission TYPE (Flexi / Fix), Real* mirror included.</summary>
        private static void SetCommType(SlabDetail row, string tier, string value)
        {
            switch (tier)
            {
                case SlabAuditTier.Retailer:
                    row.RetailerCommType = value; row.RealRetailerCommType = value; break;
                case SlabAuditTier.Distributor:
                    row.DistributorCommType = value; row.RealDistributorCommType = value; break;
                case SlabAuditTier.SuperDistributor:
                    row.SuperDistributorCommType = value; row.RealSuperDistributorCommType = value; break;
                default:
                    row.ClientCommType = value; row.RealClientCommType = value; break;
            }
        }

        /// <summary>Set a tier's CATEGORY (Commission / Surcharge), Real* mirror included.</summary>
        private static void SetCategory(SlabDetail row, string tier, string value)
        {
            switch (tier)
            {
                case SlabAuditTier.Retailer:
                    row.RetailerCommissionCategory = value; row.RealRetailerCommissionCategory = value; break;
                case SlabAuditTier.Distributor:
                    row.DistributorCommissionCategory = value; row.RealDistributorCommissionCategory = value; break;
                case SlabAuditTier.SuperDistributor:
                    row.SuperDistributorCommissionCategory = value; row.RealSuperDistributorCommissionCategory = value; break;
                default:
                    row.ClientCommissionCategory = value; row.RealClientCommissionCategory = value; break;
            }
        }

        private static decimal SignedOf(SlabDetail row, string tier) =>
            SlabPayout.Signed(CommOf(row, tier), CategoryOf(row, tier));

        /// <summary>What the whole band pays out at one amount, across every tier.</summary>
        private static decimal RowTotalAt(SlabDetail row, decimal amount) =>
            SlabPayout.TotalCurve(row).At(amount);

        private static string TierLabel(string tier) => tier switch
        {
            SlabAuditTier.Retailer => "Retailer",
            SlabAuditTier.Distributor => "Distributor",
            SlabAuditTier.SuperDistributor => "Super Distributor",
            _ => "Client",
        };

        // ── Finding the card above ──────────────────────────────────────────────────────────

        /// <summary>
        /// The card that caps <paramref name="userID"/>: their own assignment, or the nearest
        /// upline's if they hold none. Walked exactly the way
        /// <see cref="SlabCommissionResolver.FindSlabDetail"/> walks it, so the ceiling this service
        /// enforces is the card the engine would actually have paid them from.
        ///
        /// A card the user OWNS is skipped. Without that, a distributor who had somehow been
        /// assigned one of its own child cards would be validating its rates against itself and
        /// could raise them without limit.
        /// </summary>
        private async Task<SlabMaster?> FindCeilingSlab(int userID)
        {
            int probeID = userID;
            for (int depth = 0; depth < MaxChainWalk && probeID > 0; depth++)
            {
                var mapping = (await _mappingRepo.GetByUserID(probeID))
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.UserSlabMappingID)
                    .FirstOrDefault();
                if (mapping != null)
                {
                    var slab = await _slabMasterRepo.GetByID(mapping.SlabMasterID);
                    if (slab != null && slab.OwnerUserID != userID) return slab;
                }
                var user = await _userRepo.GetUserById(probeID);
                probeID = user?.ParentID ?? 0;
            }
            return null;
        }

        /// <summary>
        /// The band on <paramref name="parentSlabID"/> that governs a child band. Matched on the
        /// identity of the band — service, operator and the exact amount range — because Rule 4
        /// freezes those four on a child, so a child band always has exactly one parent band or
        /// none at all. The stored ParentSlabDetailID is preferred and this is the fallback for a
        /// card whose parent rows were re-saved (an EF update writes a new row id on some paths).
        /// </summary>
        private async Task<SlabDetail?> FindParentBand(int parentSlabID, SlabDetail child)
        {
            var parentRows = await _slabDetailRepo.GetBySlabMasterID(parentSlabID);
            if (child.ParentSlabDetailID is int pid && pid > 0)
            {
                var exact = parentRows.FirstOrDefault(r => r.SlabDetailID == pid && !r.IsDeleted);
                if (exact != null) return exact;
            }
            return parentRows.FirstOrDefault(r =>
                !r.IsDeleted && r.ServiceID == child.ServiceID && r.OperatorID == child.OperatorID &&
                r.FromAmount == child.FromAmount && r.ToAmount == child.ToAmount);
        }

        // ── Validation ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Whether <paramref name="child"/> stays inside what the parent CARD allows, and if not,
        /// the sentence the owner is shown. Empty string = inside.
        ///
        /// This used to pair a child row with one parent row of identical service, operator and
        /// amount range, and compare them tier by tier. That only worked while a child was
        /// forbidden from choosing its own bands. Now that it may split a parent band and pick its
        /// own flat-or-percent type, there is no row to pair with — so the comparison is against
        /// the parent's whole card, evaluated as money:
        ///
        ///   * each side's payout is a straight line in the transaction amount
        ///     (<see cref="PayoutCurve"/>), so comparing at the endpoints of every stretch where
        ///     one parent band governs is exact rather than a sample;
        ///   * a gap in the parent's coverage is a refusal, not a free hand — a child cannot price
        ///     an amount its parent does not price at all;
        ///   * GST still follows the parent (Rule 5), because the tax treatment of a charge is not
        ///     the reseller's to re-decide.
        ///
        /// The per-tier ceiling of the old rule is gone on purpose, and so is the row-total one
        /// that replaced it. What bounds a card is what its OWNER is paid: the sum of the tiers
        /// the card hands out, measured against the owner's own line on the card above.
        /// </summary>
        private static string ValidateBand(
            SlabDetail child, List<SlabDetail> candidates, SlabDetail? enclosing, int ownerUserType)
        {
            if (child.FromAmount < 0 || child.ToAmount < child.FromAmount)
                return "Enter a valid amount range.";

            if (enclosing != null
                && (!string.Equals(ChargeGstType.Normalize(enclosing.GSTType), ChargeGstType.Normalize(child.GSTType),
                        StringComparison.OrdinalIgnoreCase)
                    || enclosing.GSTRatePercent != child.GSTRatePercent))
                return SlabDelegationPolicy.Message.GstLocked;

            return SlabPayout.ValidateAgainstParent(child, candidates, child.FromAmount, child.ToAmount,
                SlabDelegationPolicy.OwnTierOf(ownerUserType));
        }

        /// <summary>
        /// Hold the owner's own column at zero on a card they wrote.
        ///
        /// It used to carry their computed margin, which was wrong in a way that only shows up in
        /// the ledger: the engine pays a distributor from the card assigned to the DISTRIBUTOR,
        /// not from the card the distributor wrote for its retailers. A figure here was therefore
        /// either ignored or, worse, double-counted against the owner's own ceiling. The margin
        /// needs no column — it is what is left of the credit the platform posts to the owner
        /// after the debit for what they passed down.
        /// </summary>
        private static void ClearOwnTier(SlabDetail row, int ownerUserType)
        {
            string? ownTier = SlabDelegationPolicy.OwnTierOf(ownerUserType);
            if (ownTier == null) return; // admin's card answers to nobody

            SetComm(row, ownTier, 0m);
            SetCommType(row, ownTier, CommTypeFix);
            SetCategory(row, ownTier, CategoryCommission);
        }

        /// <summary>
        /// Two bands on ONE card must never both match a transaction amount for the same service
        /// and operator — if they did, which one the engine paid would come down to row order.
        /// Mirrors the rule the admin slab editor already enforces (SlabDetailService.ValidateRange);
        /// it has to be re-checked here because a reseller sets its own ranges now.
        /// </summary>
        private async Task<string> ValidateNoOverlap(SlabDetail row)
        {
            var siblings = (await _slabDetailRepo.GetBySlabMasterID(row.SlabMasterID))
                .Where(r => !r.IsDeleted
                            && r.SlabDetailID != row.SlabDetailID
                            && r.ServiceID == row.ServiceID
                            && r.OperatorID == row.OperatorID)
                .ToList();

            var clash = siblings.FirstOrDefault(r =>
                row.FromAmount <= r.ToAmount && row.ToAmount >= r.FromAmount);
            if (clash == null) return string.Empty;

            decimal nextFrom = siblings.Max(r => r.ToAmount) + 1;
            return $"This range overlaps your ₹{clash.FromAmount:0.##} – ₹{clash.ToAmount:0.##} band. "
                 + $"The next band can start at ₹{nextFrom:0.##}.";
        }

        private const string CommTypeFlexi = "Flexi";
        private const string CommTypeFix = "Fix";
        private const string CategorySurcharge = "Surcharge";
        private const string CategoryCommission = "Commission";

        // ── Reads ───────────────────────────────────────────────────────────────────────────

        public async Task<(string failureMessage, DelegatedSlabView? data)> MyParentSlab(int callerUserID, int callerTypeID)
        {
            try
            {
                var ceiling = await FindCeilingSlab(callerUserID);
                if (ceiling == null) return (SlabDelegationPolicy.Message.NoParentSlab, null);

                var view = await BuildView(ceiling, parentSlab: null, callerTypeID, editable: false);
                return (StatusType.Ok, view);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, null); }
        }

        public async Task<(string failureMessage, List<DelegatedSlabSummary> data)> MySlabs(int callerUserID, int callerTypeID)
        {
            try
            {
                var all = await _slabMasterRepo.GetAll();
                // Admin sees every card there is — its own, and every reseller's, which is the
                // oversight half of delegation. A downline sees only the cards it owns.
                var mine = IsAdminTier(callerTypeID)
                    ? all
                    : all.Where(s => s.OwnerUserID == callerUserID).ToList();

                var result = new List<DelegatedSlabSummary>();
                foreach (var slab in mine.OrderBy(s => s.SlabName))
                    result.Add(await BuildSummary(slab, all));

                return (StatusType.Ok, result);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, new List<DelegatedSlabSummary>()); }
        }

        public async Task<(string failureMessage, DelegatedSlabView? data)> SlabView(int slabMasterID, int callerUserID, int callerTypeID)
        {
            try
            {
                var slab = await _slabMasterRepo.GetByID(slabMasterID);
                if (slab == null) return (Messages.SlabMasterMessages.NotFound, null);

                bool admin = IsAdminTier(callerTypeID);
                if (!admin && slab.OwnerUserID != callerUserID)
                    return (SlabDelegationPolicy.Message.NotYourSlab, null);

                SlabMaster? parent = slab.ParentSlabID is int pid && pid > 0
                    ? await _slabMasterRepo.GetByID(pid) : null;

                // Admin may READ a reseller's card but does not edit it from this screen — the
                // owner's margin is the owner's business, and an admin edit here would silently
                // overwrite a decision the owner is answerable for. Admin's lever is its own card,
                // which cascades down through revalidation.
                bool editable = slab.OwnerUserID == callerUserID && parent != null;
                var view = await BuildView(slab, parent, callerTypeID, editable);
                return (StatusType.Ok, view);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, null); }
        }

        private async Task<DelegatedSlabSummary> BuildSummary(SlabMaster slab, List<SlabMaster> allSlabs)
        {
            var bands = await _slabDetailRepo.GetBySlabMasterID(slab.SlabMasterID);
            var owner = slab.OwnerUserID is int oid && oid > 0 ? await _userRepo.GetUserById(oid) : null;
            var assignments = (await _mappingRepo.GetAll())
                .Where(m => !m.IsDeleted && m.SlabMasterID == slab.SlabMasterID).ToList();

            return new DelegatedSlabSummary
            {
                SlabMasterID = slab.SlabMasterID,
                SlabName = slab.SlabName,
                ParentSlabID = slab.ParentSlabID,
                ParentSlabName = slab.ParentSlabID is int p
                    ? allSlabs.FirstOrDefault(s => s.SlabMasterID == p)?.SlabName ?? string.Empty
                    : string.Empty,
                OwnerUserID = slab.OwnerUserID,
                OwnerName = owner == null ? "Admin" : DisplayName(owner),
                OwnerUserType = slab.OwnerUserType,
                IsDelegatedSlab = slab.IsDelegatedSlab,
                IsActive = slab.IsActive,
                Status = SlabRowStatus.Normalize(slab.Status),
                BandCount = bands.Count,
                InvalidBandCount = bands.Count(b => SlabRowStatus.IsInvalid(b.Status)),
                AssignedUserCount = assignments.Select(m => m.UserID).Distinct().Count(),
                DateTime = slab.DateTime,
            };
        }

        private async Task<DelegatedSlabView> BuildView(SlabMaster slab, SlabMaster? parentSlab, int callerTypeID, bool editable)
        {
            var allSlabs = await _slabMasterRepo.GetAll();
            var bands = await _slabDetailRepo.GetBySlabMasterID(slab.SlabMasterID);
            var services = await _serviceRepo.GetAllServices();
            var operators = await _operatorRepo.GetAllOperators();

            // The parent's bands, re-read live rather than taken from the child's stored snapshot:
            // the snapshot records what the ceiling WAS at save time (which is what the audit
            // needs), and the editor has to show what it IS.
            var parentBands = parentSlab != null
                ? await _slabDetailRepo.GetBySlabMasterID(parentSlab.SlabMasterID)
                : new List<SlabDetail>();

            var view = new DelegatedSlabView
            {
                Slab = await BuildSummary(slab, allSlabs),
                OwnTier = editable ? SlabDelegationPolicy.OwnTierOf(slab.OwnerUserType) ?? string.Empty : string.Empty,
                // Driven by the OWNER's tier, not the viewer's: an admin reading a distributor's
                // card should see the one column that card's owner decides, not all four.
                SettableTiers = editable
                    ? SlabDelegationPolicy.TiersSettableBy(slab.OwnerUserType).ToList()
                    : new List<string>(),
                IsEditable = editable,
            };

            string? ownTierName = SlabDelegationPolicy.OwnTierOf(slab.OwnerUserType);
            var liveParentBands = parentBands.Where(r => !r.IsDeleted).ToList();

            foreach (var b in bands.Where(x => !x.IsDeleted))
            {
                // The parent band that governs where this one STARTS. Resolved by the engine's own
                // precedence rather than by looking for an identical range, because a child band
                // may now be a slice of a parent band rather than a copy of it.
                var pb = SlabPayout.ParentAt(SlabPayout.CandidateParents(liveParentBands, b), b.FromAmount);

                view.Bands.Add(new DelegatedSlabBand
                {
                    SlabDetailID = b.SlabDetailID,
                    ParentSlabDetailID = b.ParentSlabDetailID,
                    ServiceID = b.ServiceID,
                    ServiceName = services.FirstOrDefault(s => s.ServiceID == b.ServiceID)?.Name ?? string.Empty,
                    OperatorID = b.OperatorID,
                    OperatorName = operators.FirstOrDefault(o => o.OperatorID == b.OperatorID)?.OperatorName ?? string.Empty,
                    FromAmount = b.FromAmount,
                    ToAmount = b.ToAmount,
                    RetailerComm = b.RetailerComm,
                    DistributorComm = b.DistributorComm,
                    SuperDistributorComm = b.SuperDistributorComm,
                    ClientComm = b.ClientComm,
                    RetailerCommType = b.RetailerCommType,
                    DistributorCommType = b.DistributorCommType,
                    SuperDistributorCommType = b.SuperDistributorCommType,
                    ClientCommType = b.ClientCommType,
                    RetailerCommissionCategory = b.RetailerCommissionCategory,
                    DistributorCommissionCategory = b.DistributorCommissionCategory,
                    SuperDistributorCommissionCategory = b.SuperDistributorCommissionCategory,
                    ClientCommissionCategory = b.ClientCommissionCategory,
                    ParentRetailerComm = pb?.RetailerComm ?? 0,
                    ParentDistributorComm = pb?.DistributorComm ?? 0,
                    ParentSuperDistributorComm = pb?.SuperDistributorComm ?? 0,
                    ParentClientComm = pb?.ClientComm ?? 0,
                    AllowanceAtFrom = pb == null ? 0 : SlabPayout.AllowanceCurve(pb, ownTierName).At(b.FromAmount),
                    AllowanceAtTo = pb == null ? 0 : SlabPayout.AllowanceCurve(pb, ownTierName).At(b.ToAmount),
                    GivenAwayAtFrom = SlabPayout.OutlayCurve(b, ownTierName).At(b.FromAmount),
                    GivenAwayAtTo = SlabPayout.OutlayCurve(b, ownTierName).At(b.ToAmount),
                    MarginAtFrom = (pb == null ? 0 : SlabPayout.AllowanceCurve(pb, ownTierName).At(b.FromAmount))
                                 - SlabPayout.OutlayCurve(b, ownTierName).At(b.FromAmount),
                    MarginAtTo = (pb == null ? 0 : SlabPayout.AllowanceCurve(pb, ownTierName).At(b.ToAmount))
                               - SlabPayout.OutlayCurve(b, ownTierName).At(b.ToAmount),
                    GSTType = ChargeGstType.Normalize(b.GSTType),
                    GSTRatePercent = b.GSTRatePercent,
                    Status = SlabRowStatus.Normalize(b.Status),
                    // Re-derived rather than stored, so the reason shown is measured against the
                    // parent card as it stands right now.
                    StatusReason = SlabRowStatus.IsInvalid(b.Status) && parentSlab != null
                        ? SlabPayout.ValidateAgainstParent(
                            b, SlabPayout.CandidateParents(parentBands, b), b.FromAmount, b.ToAmount, ownTierName)
                        : string.Empty,
                    IsActive = b.IsActive,
                });
            }

            view.Bands = view.Bands
                .OrderBy(x => x.ServiceName).ThenBy(x => x.OperatorName).ThenBy(x => x.FromAmount)
                .ToList();
            return view;
        }

        private static string DisplayName(UserMaster u)
        {
            string name = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName
                : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName!
                : $"User {u.UserMasterID}";
            return string.IsNullOrWhiteSpace(u.MobileNumber) ? name : $"{name} [{u.MobileNumber}]";
        }

        // ── Creating a child card ───────────────────────────────────────────────────────────

        public async Task<(string failureMessage, int slabMasterID)> CreateChildSlab(
            CreateChildSlabRequest request, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SlabName))
                    return ("Please enter a name for the slab.", 0);
                if (!SlabDelegationPolicy.CanCreateSlab(callerTypeID))
                    return (SlabDelegationPolicy.Message.Unauthorized, 0);
                if (request.ForUserTypeID > 0 && !SlabDelegationPolicy.CanAssignTo(callerTypeID, request.ForUserTypeID))
                    return (SlabDelegationPolicy.Message.Unauthorized, 0);

                var ceiling = await FindCeilingSlab(callerUserID);
                if (ceiling == null) return (SlabDelegationPolicy.Message.NoParentSlab, 0);

                string name = request.SlabName.Trim();
                var existing = await _slabMasterRepo.GetByName(name);
                if (existing != null) return (Messages.SlabMasterMessages.AlreadyExists, 0);

                var caller = await _userRepo.GetUserById(callerUserID);

                var child = new SlabMaster
                {
                    SlabName = name,
                    IsActive = true,
                    ParentSlabID = ceiling.SlabMasterID,
                    OwnerUserID = callerUserID,
                    OwnerUserType = callerTypeID,
                    CreatedByUserID = callerUserID,
                    IsDelegatedSlab = true,
                    Status = SlabRowStatus.Active,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                };
                if (await _slabMasterRepo.Insert(child) <= 0)
                    return (Messages.Common.Message.Failed, 0);

                // Clone every band of the ceiling card at PASS-THROUGH: the owner's own commission
                // for each band goes straight down to the tier this card is for, and the owner
                // keeps nothing. A card created and left alone therefore changes no economics —
                // the only safe default when the thing being created is a price list — and the one
                // decision the owner then makes is how much of it to keep.
                var parentBands = (await _slabDetailRepo.GetBySlabMasterID(ceiling.SlabMasterID))
                    .Where(b => !b.IsDeleted).ToList();

                var ownerSettable = SlabDelegationPolicy.TiersSettableBy(callerTypeID);
                string passTier = request.ForUserTypeID > 0
                    ? SlabDelegationPolicy.TierForRole(request.ForUserTypeID)
                    : SlabDelegationPolicy.DefaultPassThroughTier(callerTypeID);
                if (!ownerSettable.Contains(passTier))
                    passTier = SlabDelegationPolicy.DefaultPassThroughTier(callerTypeID);

                foreach (var pb in parentBands)
                    await _slabDetailRepo.Insert(BandFrom(pb, child.SlabMasterID, callerTypeID, passTier));

                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = child.SlabMasterID,
                    OwnerUserID = callerUserID,
                    ParentUserID = caller?.ParentID,
                    UpdatedByUserID = callerUserID,
                    Tier = SlabAuditTier.Create,
                    Remark = $"Created \"{name}\" from \"{ceiling.SlabName}\" with {parentBands.Count} band(s).",
                    IPAddress = ipAddress,
                });

                return (StatusType.Ok, child.SlabMasterID);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, 0); }
        }

        /// <summary>
        /// A child band cut from <paramref name="pb"/>, carrying the parent's range and GST
        /// decision, and seeded at PASS-THROUGH: the owner's whole commission for that band goes
        /// straight to <paramref name="passTier"/> and the owner keeps nothing.
        ///
        /// Pass-through is the only safe default for a price list. Creating a card changes no
        /// economics — the tier below earns exactly what the owner earns — until the owner decides
        /// to keep something, which is the one decision the screen asks them for. Seeding it at
        /// the parent's own COLUMNS instead (what this did before) copied figures that are the
        /// parent's costs, not the owner's income, and produced cards that were over the limit the
        /// moment they were created.
        /// </summary>
        private static SlabDetail BandFrom(SlabDetail pb, int childSlabMasterID, int ownerUserType, string? passTier)
        {
            string? ownTier = SlabDelegationPolicy.OwnTierOf(ownerUserType);
            var row = new SlabDetail
            {
                SlabMasterID = childSlabMasterID,
                ServiceID = pb.ServiceID,
                OperatorID = pb.OperatorID,
                FromAmount = pb.FromAmount,
                ToAmount = pb.ToAmount,

                GSTType = pb.GSTType,
                GSTRatePercent = pb.GSTRatePercent,

                ParentSlabDetailID = pb.SlabDetailID,
                ParentRetailerComm = pb.RetailerComm,
                ParentDistributorComm = pb.DistributorComm,
                ParentSuperDistributorComm = pb.SuperDistributorComm,
                ParentClientComm = pb.ClientComm,
                ParentCommType = pb.RetailerCommType,
                ParentGSTType = pb.GSTType,
                ParentGSTRatePercent = pb.GSTRatePercent,

                Status = SlabRowStatus.Active,
                IsActive = pb.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };

            // Every column starts empty, including the owner's own.
            foreach (var tier in Tiers)
            {
                SetComm(row, tier, 0m);
                SetCommType(row, tier, CommTypeFix);
                SetCategory(row, tier, CategoryCommission);
            }

            // The owner's whole allowance, handed down whole. A single column always holds it:
            // the allowance is one tier of one parent band, so it is flat or percent, never both.
            // A null passTier means "leave the band empty" — used when the owner is typing the
            // figures themselves and a seeded column would silently add to what they entered.
            if (passTier != null)
            {
                var allowance = SlabPayout.AllowanceCurve(pb, ownTier);
                bool asPercent = allowance.Percent != 0m;
                decimal value = asPercent ? allowance.Percent : allowance.Flat;
                SetComm(row, passTier, Math.Abs(value));
                SetCommType(row, passTier, asPercent ? CommTypeFlexi : CommTypeFix);
                SetCategory(row, passTier, value < 0 ? CategorySurcharge : CategoryCommission);
            }

            // The legacy "Real" columns mirror the live ones on a new row.
            row.RealRetailerComm = row.RetailerComm;
            row.RealRetailerCommType = row.RetailerCommType;
            row.RealRetailerCommissionCategory = row.RetailerCommissionCategory;
            row.RealDistributorComm = row.DistributorComm;
            row.RealDistributorCommType = row.DistributorCommType;
            row.RealDistributorCommissionCategory = row.DistributorCommissionCategory;
            row.RealSuperDistributorComm = row.SuperDistributorComm;
            row.RealSuperDistributorCommType = row.SuperDistributorCommType;
            row.RealSuperDistributorCommissionCategory = row.SuperDistributorCommissionCategory;
            row.RealClientComm = row.ClientComm;
            row.RealClientCommType = row.ClientCommType;
            row.RealClientCommissionCategory = row.ClientCommissionCategory;

            return row;
        }

        /// <summary>
        /// Clone down any band the parent card has and <paramref name="child"/> does not. Returns
        /// how many were added.
        /// </summary>
        private async Task<int> CloneMissingBands(List<SlabDetail> parentBands, SlabMaster child)
        {
            var existing = (await _slabDetailRepo.GetBySlabMasterID(child.SlabMasterID))
                .Where(b => !b.IsDeleted).ToList();

            int added = 0;
            foreach (var pb in parentBands)
            {
                bool present = existing.Any(b =>
                    (b.ParentSlabDetailID is int pid && pid == pb.SlabDetailID) ||
                    (b.ServiceID == pb.ServiceID && b.OperatorID == pb.OperatorID &&
                     b.FromAmount == pb.FromAmount && b.ToAmount == pb.ToAmount));
                if (present) continue;
                if (await _slabDetailRepo.Insert(BandFrom(pb, child.SlabMasterID, child.OwnerUserType,
                        SlabDelegationPolicy.DefaultPassThroughTier(child.OwnerUserType))) > 0) added++;
            }
            return added;
        }

        // ── Moving a figure ─────────────────────────────────────────────────────────────────

        public async Task<string> UpdateRow(UpdateDelegatedRowRequest request, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                var row = await _slabDetailRepo.GetByID(request.SlabDetailID);
                if (row == null || row.IsDeleted) return Messages.SlabDetailMessages.NotFound;

                var slab = await _slabMasterRepo.GetByID(row.SlabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;
                if (slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;
                if (slab.ParentSlabID is not int parentSlabID || parentSlabID <= 0)
                    return SlabDelegationPolicy.Message.NoParentSlab;

                // Which columns this owner may set at all — the tiers strictly BELOW them. A
                // distributor sells to retailers, so a retailer rate is the only rate it decides;
                // a super-distributor decides the distributor and retailer rates. Their own column
                // is absent because it is computed, and the columns above them are not their money.
                var settable = SlabDelegationPolicy.TiersSettableBy(slab.OwnerUserType);
                if (settable.Length == 0) return SlabDelegationPolicy.Message.NothingSettable;

                // Build the candidate off a COPY, validate it, and only then write. The alternative
                // — mutate, validate, roll back on failure — leaves the tracked entity dirty on a
                // refusal, and the next save on the same request writes the rejected figures.
                var candidate = Clone(row);

                // The band itself, where the owner moved it. Re-banding and re-typing are the
                // reseller's to choose now; what bounds them is the ceiling check below, which
                // compares the two cards as MONEY over the amount range rather than row to row.
                if (request.FromAmount.HasValue) candidate.FromAmount = request.FromAmount.Value;
                if (request.ToAmount.HasValue) candidate.ToAmount = request.ToAmount.Value;

                foreach (var tier in settable)
                {
                    if (!request.Tiers.TryGetValue(tier, out var set)) continue;
                    SetComm(candidate, tier, Math.Abs(set.Comm));
                    if (!string.IsNullOrWhiteSpace(set.CommType))
                        SetCommType(candidate, tier, SlabCommissionResolver.IsPercentType(set.CommType) ? CommTypeFlexi : CommTypeFix);
                    if (!string.IsNullOrWhiteSpace(set.Category))
                        SetCategory(candidate, tier, SlabCommissionResolver.IsSurchargeCategory(set.Category) ? CategorySurcharge : CategoryCommission);
                }

                // The amount range may have moved, so the governing parent band is resolved from
                // the card rather than from the row's stored link.
                var parentBands = (await _slabDetailRepo.GetBySlabMasterID(parentSlabID))
                    .Where(b => !b.IsDeleted).ToList();
                var candidates = SlabPayout.CandidateParents(parentBands, candidate);
                var (enclosing, enclosingFail) = SlabPayout.EnclosingParent(
                    candidates, candidate.FromAmount, candidate.ToAmount);
                if (enclosing == null) return enclosingFail;

                // GST follows the parent band, always.
                candidate.GSTType = enclosing.GSTType;
                candidate.GSTRatePercent = enclosing.GSTRatePercent;

                // The owner's own column stays empty: their margin is the gap between the credit
                // the platform posts them and the debit for what they passed down, not a figure
                // on the card they wrote.
                ClearOwnTier(candidate, slab.OwnerUserType);

                string invalid = ValidateBand(candidate, candidates, enclosing, slab.OwnerUserType);
                if (!string.IsNullOrEmpty(invalid)) return invalid;

                // Two bands of one card may never both match an amount, or the engine's answer
                // depends on row order. Checked after the ceiling so the clearer refusal wins.
                string overlap = await ValidateNoOverlap(candidate);
                if (!string.IsNullOrEmpty(overlap)) return overlap;

                var parentBand = enclosing;

                // One audit row per figure that actually moved, carrying the ceiling it was
                // measured against. A save that moved nothing writes nothing.
                var caller = await _userRepo.GetUserById(callerUserID);
                var audits = new List<SlabCommissionAudit>();
                foreach (var tier in Tiers)
                {
                    decimal before = CommOf(row, tier);
                    decimal after = CommOf(candidate, tier);
                    if (before == after) continue;

                    audits.Add(new SlabCommissionAudit
                    {
                        SlabMasterID = slab.SlabMasterID,
                        SlabDetailID = row.SlabDetailID,
                        OwnerUserID = slab.OwnerUserID,
                        ParentUserID = caller?.ParentID,
                        UpdatedByUserID = callerUserID,
                        ServiceID = row.ServiceID,
                        OperatorID = row.OperatorID,
                        FromAmount = row.FromAmount,
                        ToAmount = row.ToAmount,
                        Tier = tier,
                        OldCommission = before,
                        NewCommission = after,
                        OldCommType = TypeOf(row, tier),
                        NewCommType = TypeOf(candidate, tier),
                        ParentCommission = CommOf(parentBand, tier),
                        IPAddress = ipAddress,
                        DateTime = DateTime.UtcNow,
                    });
                }
                // A re-band with the SAME rates moves no tier figure, so the audit list stays
                // empty — and returning early on that alone silently threw the new range away.
                // "Nothing changed" has to mean the range as well as the rates now.
                bool reBanded = candidate.FromAmount != row.FromAmount || candidate.ToAmount != row.ToAmount;
                if (reBanded)
                {
                    audits.Add(new SlabCommissionAudit
                    {
                        SlabMasterID = slab.SlabMasterID,
                        SlabDetailID = row.SlabDetailID,
                        OwnerUserID = slab.OwnerUserID,
                        ParentUserID = caller?.ParentID,
                        UpdatedByUserID = callerUserID,
                        ServiceID = row.ServiceID,
                        OperatorID = row.OperatorID,
                        FromAmount = candidate.FromAmount,
                        ToAmount = candidate.ToAmount,
                        Tier = SlabAuditTier.Retailer,
                        OldCommission = CommOf(row, SlabAuditTier.Retailer),
                        NewCommission = CommOf(candidate, SlabAuditTier.Retailer),
                        ParentCommission = CommOf(parentBand, SlabAuditTier.Retailer),
                        Remark = $"Band moved from ₹{row.FromAmount:0.##} – ₹{row.ToAmount:0.##} "
                               + $"to ₹{candidate.FromAmount:0.##} – ₹{candidate.ToAmount:0.##}.",
                        IPAddress = ipAddress,
                        DateTime = DateTime.UtcNow,
                    });
                }

                if (audits.Count == 0) return StatusType.Ok;

                // Re-snapshot the ceiling: these figures were just measured against TODAY's parent,
                // so that is the limit the row now records having been saved under.
                candidate.ParentSlabDetailID = parentBand.SlabDetailID;
                candidate.ParentRetailerComm = parentBand.RetailerComm;
                candidate.ParentDistributorComm = parentBand.DistributorComm;
                candidate.ParentSuperDistributorComm = parentBand.SuperDistributorComm;
                candidate.ParentClientComm = parentBand.ClientComm;
                candidate.ParentCommType = parentBand.RetailerCommType;
                candidate.ParentGSTType = parentBand.GSTType;
                candidate.ParentGSTRatePercent = parentBand.GSTRatePercent;
                candidate.Status = SlabRowStatus.Active;
                candidate.UpdatedateTime = DateTime.UtcNow;

                if (await _slabDetailRepo.Update(candidate) <= 0) return Messages.Common.Message.Failed;

                await AuditRange(audits);
                await RollUpStatus(slab.SlabMasterID);

                // A distributor lowering a retailer figure constrains its OWN children the same way
                // an admin edit constrains the distributor. Nothing about cascading is admin-only.
                await RevalidateChildren(slab.SlabMasterID, callerUserID, ipAddress);

                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        private static SlabDetail Clone(SlabDetail s) => new()
        {
            SlabDetailID = s.SlabDetailID,
            SlabMasterID = s.SlabMasterID,
            ServiceID = s.ServiceID,
            OperatorID = s.OperatorID,
            FromAmount = s.FromAmount,
            ToAmount = s.ToAmount,
            RetailerComm = s.RetailerComm,
            RetailerCommType = s.RetailerCommType,
            RetailerCommissionCategory = s.RetailerCommissionCategory,
            DistributorComm = s.DistributorComm,
            DistributorCommType = s.DistributorCommType,
            DistributorCommissionCategory = s.DistributorCommissionCategory,
            SuperDistributorComm = s.SuperDistributorComm,
            SuperDistributorCommType = s.SuperDistributorCommType,
            SuperDistributorCommissionCategory = s.SuperDistributorCommissionCategory,
            ClientComm = s.ClientComm,
            ClientCommType = s.ClientCommType,
            ClientCommissionCategory = s.ClientCommissionCategory,
            RealRetailerComm = s.RealRetailerComm,
            RealRetailerCommType = s.RealRetailerCommType,
            RealRetailerCommissionCategory = s.RealRetailerCommissionCategory,
            RealDistributorComm = s.RealDistributorComm,
            RealDistributorCommType = s.RealDistributorCommType,
            RealDistributorCommissionCategory = s.RealDistributorCommissionCategory,
            RealSuperDistributorComm = s.RealSuperDistributorComm,
            RealSuperDistributorCommType = s.RealSuperDistributorCommType,
            RealSuperDistributorCommissionCategory = s.RealSuperDistributorCommissionCategory,
            RealClientComm = s.RealClientComm,
            RealClientCommType = s.RealClientCommType,
            RealClientCommissionCategory = s.RealClientCommissionCategory,
            GSTType = s.GSTType,
            GSTRatePercent = s.GSTRatePercent,
            ParentSlabDetailID = s.ParentSlabDetailID,
            ParentRetailerComm = s.ParentRetailerComm,
            ParentDistributorComm = s.ParentDistributorComm,
            ParentSuperDistributorComm = s.ParentSuperDistributorComm,
            ParentClientComm = s.ParentClientComm,
            ParentCommType = s.ParentCommType,
            ParentGSTType = s.ParentGSTType,
            ParentGSTRatePercent = s.ParentGSTRatePercent,
            Status = s.Status,
            IsActive = s.IsActive,
            DateTime = s.DateTime,
            UpdatedateTime = s.UpdatedateTime,
            IsDeleted = s.IsDeleted,
        };

        public async Task<string> CreateRow(CreateDelegatedRowRequest request, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                var slab = await _slabMasterRepo.GetByID(request.SlabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;
                if (slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;
                if (slab.ParentSlabID is not int parentSlabID || parentSlabID <= 0)
                    return SlabDelegationPolicy.Message.NoParentSlab;

                var settable = SlabDelegationPolicy.TiersSettableBy(slab.OwnerUserType);
                if (settable.Length == 0) return SlabDelegationPolicy.Message.NothingSettable;

                var parentBands = (await _slabDetailRepo.GetBySlabMasterID(parentSlabID))
                    .Where(b => !b.IsDeleted).ToList();

                // The shape of the new band is taken from the parent band that governs it, so the
                // types, the categories and the GST are inherited rather than invented — then the
                // owner's figures are laid over the top.
                var probe = new SlabDetail
                {
                    ServiceID = request.ServiceID,
                    OperatorID = request.OperatorID,
                    FromAmount = request.FromAmount,
                    ToAmount = request.ToAmount,
                };
                var candidates = SlabPayout.CandidateParents(parentBands, probe);
                var (enclosing, enclosingFail) = SlabPayout.EnclosingParent(
                    candidates, request.FromAmount, request.ToAmount);
                if (enclosing == null) return enclosingFail;

                var row = BandFrom(enclosing, slab.SlabMasterID, slab.OwnerUserType, passTier: null);
                row.ServiceID = request.ServiceID;
                row.OperatorID = request.OperatorID;
                row.FromAmount = request.FromAmount;
                row.ToAmount = request.ToAmount;

                foreach (var tier in settable)
                {
                    if (!request.Tiers.TryGetValue(tier, out var set)) continue;
                    SetComm(row, tier, Math.Abs(set.Comm));
                    if (!string.IsNullOrWhiteSpace(set.CommType))
                        SetCommType(row, tier, SlabCommissionResolver.IsPercentType(set.CommType) ? CommTypeFlexi : CommTypeFix);
                    if (!string.IsNullOrWhiteSpace(set.Category))
                        SetCategory(row, tier, SlabCommissionResolver.IsSurchargeCategory(set.Category) ? CategorySurcharge : CategoryCommission);
                }

                ClearOwnTier(row, slab.OwnerUserType);

                string invalid = ValidateBand(row, candidates, enclosing, slab.OwnerUserType);
                if (!string.IsNullOrEmpty(invalid)) return invalid;

                string overlap = await ValidateNoOverlap(row);
                if (!string.IsNullOrEmpty(overlap)) return overlap;

                if (await _slabDetailRepo.Insert(row) <= 0) return Messages.Common.Message.Failed;

                var caller = await _userRepo.GetUserById(callerUserID);
                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = slab.SlabMasterID,
                    SlabDetailID = row.SlabDetailID,
                    OwnerUserID = slab.OwnerUserID,
                    ParentUserID = caller?.ParentID,
                    UpdatedByUserID = callerUserID,
                    ServiceID = row.ServiceID,
                    OperatorID = row.OperatorID,
                    FromAmount = row.FromAmount,
                    ToAmount = row.ToAmount,
                    Tier = SlabAuditTier.Create,
                    ParentCommission = CommOf(enclosing, SlabAuditTier.Retailer),
                    Remark = $"Added band ₹{row.FromAmount:0.##} – ₹{row.ToAmount:0.##}.",
                    IPAddress = ipAddress,
                });

                await RollUpStatus(slab.SlabMasterID);
                await RevalidateChildren(slab.SlabMasterID, callerUserID, ipAddress);
                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        public async Task<string> DeleteRow(int slabDetailID, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                var row = await _slabDetailRepo.GetByID(slabDetailID);
                if (row == null || row.IsDeleted) return Messages.SlabDetailMessages.NotFound;

                var slab = await _slabMasterRepo.GetByID(row.SlabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;
                if (slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;

                if (await _slabDetailRepo.Delete(slabDetailID) <= 0) return Messages.Common.Message.Failed;

                var caller = await _userRepo.GetUserById(callerUserID);
                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = slab.SlabMasterID,
                    SlabDetailID = slabDetailID,
                    OwnerUserID = slab.OwnerUserID,
                    ParentUserID = caller?.ParentID,
                    UpdatedByUserID = callerUserID,
                    ServiceID = row.ServiceID,
                    OperatorID = row.OperatorID,
                    FromAmount = row.FromAmount,
                    ToAmount = row.ToAmount,
                    Tier = SlabAuditTier.Retailer,
                    OldCommission = CommOf(row, SlabAuditTier.Retailer),
                    NewCommission = 0m,
                    Remark = $"Removed band ₹{row.FromAmount:0.##} – ₹{row.ToAmount:0.##}. These amounts now follow the upline's slab.",
                    IPAddress = ipAddress,
                });

                await RollUpStatus(slab.SlabMasterID);
                await RevalidateChildren(slab.SlabMasterID, callerUserID, ipAddress);
                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        /// <summary>
        /// Remove one of the caller's own cards.
        /// </summary>
        /// <remarks>
        /// A reseller could cut cards but never remove one, so a card made by mistake, or for a
        /// deal that never happened, stayed in the list and in the assign picker for good.
        ///
        /// What it will not do is take anyone's rates away behind their back. While a user is
        /// assigned to the card they are earning on it, and while another card has been cut from
        /// it that card is priced against it — so both cases are refused with the count, and the
        /// owner unassigns (or deletes the child) first. The bands go with the card, since a band
        /// of a deleted card is not something any screen can reach or any payout can read.
        /// </remarks>
        public async Task<string> DeleteSlab(int slabMasterID, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                if (slabMasterID <= 0) return Messages.Common.Message.InvalidRequest;

                var slab = await _slabMasterRepo.GetByID(slabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;

                bool admin = IsAdminTier(callerTypeID);
                if (!admin && slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;
                // The platform's own ceiling card has no owner, and removing one is a different act
                // with different consequences — it belongs to the admin's Commission Slabs screen.
                if (slab.OwnerUserID is null or 0) return SlabDelegationPolicy.Message.NotADelegatedSlab;

                int assigned = (await _mappingRepo.GetAll()).Count(m => m.SlabMasterID == slabMasterID);
                if (assigned > 0)
                    return string.Format(
                        SlabDelegationPolicy.Message.SlabInUse, assigned, assigned == 1 ? "user" : "users");

                int children = (await _slabMasterRepo.GetAll()).Count(s => s.ParentSlabID == slabMasterID);
                if (children > 0)
                    return string.Format(
                        SlabDelegationPolicy.Message.SlabHasChildren,
                        children, children == 1 ? "slab has" : "slabs have");

                var bands = await _slabDetailRepo.GetBySlabMasterID(slabMasterID);
                if (await _slabMasterRepo.Delete(slabMasterID) <= 0) return Messages.Common.Message.Failed;
                // Card first, then its bands: if the card survives a failure here it is still a
                // whole card, whereas bandless card would price nothing and look fine.
                foreach (var band in bands) await _slabDetailRepo.Delete(band.SlabDetailID);

                var caller = await _userRepo.GetUserById(callerUserID);
                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = slabMasterID,
                    OwnerUserID = slab.OwnerUserID,
                    ParentUserID = caller?.ParentID,
                    UpdatedByUserID = callerUserID,
                    Tier = SlabAuditTier.Delete,
                    Remark = $"Deleted slab \"{slab.SlabName}\" and its {bands.Count} band{(bands.Count == 1 ? string.Empty : "s")}.",
                    IPAddress = ipAddress,
                });
                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        // ── Assigning ───────────────────────────────────────────────────────────────────────

        public async Task<string> AssignSlab(AssignDelegatedSlabRequest request, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                if (request.UserID <= 0 || request.SlabMasterID <= 0) return Messages.Common.Message.InvalidRequest;

                var slab = await _slabMasterRepo.GetByID(request.SlabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;

                bool admin = IsAdminTier(callerTypeID);
                if (!admin && slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;

                var target = await _userRepo.GetUserById(request.UserID);
                if (target == null || target.IsDeleted) return Messages.UserMessages.UserNotFound;

                if (!SlabDelegationPolicy.CanAssignTo(callerTypeID, target.UserTypeID ?? 0))
                    return SlabDelegationPolicy.Message.Unauthorized;

                if (!admin)
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), callerUserID);
                    if (!downline.Contains(target.UserMasterID))
                        return SlabDelegationPolicy.Message.OutsideNetwork;
                }

                // The card has to still be inside its own ceiling before it is handed to anybody.
                // Assigning an invalid card would put a user on a rate the engine refuses to pay,
                // which reads to them as the service being broken rather than the slab being stale.
                await RollUpStatus(slab.SlabMasterID);
                var refreshed = await _slabMasterRepo.GetByID(slab.SlabMasterID);
                if (refreshed != null && SlabRowStatus.IsInvalid(refreshed.Status))
                    return SlabDelegationPolicy.Message.ParentUpdated;

                var current = (await _mappingRepo.GetByUserID(target.UserMasterID))
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.UserSlabMappingID)
                    .FirstOrDefault();
                if (current != null && current.SlabMasterID == slab.SlabMasterID)
                    return StatusType.Ok; // already on it; nothing to say and nothing to log

                if (current != null)
                {
                    // One live mapping per user. The resolver takes the newest anyway, so leaving
                    // the old one behind would only make the Assign list read as two cards at once.
                    await _mappingRepo.Delete(current.UserSlabMappingID);
                }

                int inserted = await _mappingRepo.Insert(new UserSlabMapping
                {
                    UserID = target.UserMasterID,
                    SlabMasterID = slab.SlabMasterID,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
                if (inserted <= 0) return Messages.Common.Message.Failed;

                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = slab.SlabMasterID,
                    OwnerUserID = slab.OwnerUserID,
                    ParentUserID = target.ParentID,
                    UpdatedByUserID = callerUserID,
                    Tier = SlabAuditTier.Assign,
                    Remark = $"Assigned \"{slab.SlabName}\" to {DisplayName(target)}.",
                    IPAddress = ipAddress,
                });

                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        public async Task<(string failureMessage, List<DelegatedAssignment> data)> Assignments(int callerUserID, int callerTypeID)
        {
            try
            {
                var slabs = await _slabMasterRepo.GetAll();
                bool admin = IsAdminTier(callerTypeID);
                var mine = admin ? slabs : slabs.Where(s => s.OwnerUserID == callerUserID).ToList();
                var mineIds = mine.Select(s => s.SlabMasterID).ToHashSet();

                var users = await _userRepo.GetAllUsers();
                var scope = admin ? null : UserHierarchy.GetDownlineIds(users, callerUserID);
                var types = users.ToDictionary(u => u.UserMasterID, u => u);

                var rows = (await _mappingRepo.GetAll())
                    .Where(m => !m.IsDeleted && mineIds.Contains(m.SlabMasterID))
                    .Where(m => scope == null || scope.Contains(m.UserID))
                    .Select(m =>
                    {
                        types.TryGetValue(m.UserID, out var u);
                        return new DelegatedAssignment
                        {
                            UserSlabMappingID = m.UserSlabMappingID,
                            UserID = m.UserID,
                            UserName = u == null ? $"User {m.UserID}"
                                : (!string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : u.PersonName ?? $"User {m.UserID}"),
                            MobileNumber = u?.MobileNumber ?? string.Empty,
                            UserCode = u?.AliasName ?? string.Empty,
                            UserTypeID = u?.UserTypeID ?? 0,
                            UserType = RoleName(u?.UserTypeID ?? 0),
                            SlabMasterID = m.SlabMasterID,
                            SlabName = mine.FirstOrDefault(s => s.SlabMasterID == m.SlabMasterID)?.SlabName ?? string.Empty,
                            DateTime = m.DateTime,
                        };
                    })
                    .OrderBy(r => r.UserType).ThenBy(r => r.UserName)
                    .ToList();

                return (StatusType.Ok, rows);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, new List<DelegatedAssignment>()); }
        }

        public async Task<string> UnassignSlab(int userSlabMappingID, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                var mapping = await _mappingRepo.GetByID(userSlabMappingID);
                if (mapping == null || mapping.IsDeleted) return Messages.UserSlabMappingMessages.NotFound;

                var slab = await _slabMasterRepo.GetByID(mapping.SlabMasterID);
                if (slab == null) return Messages.SlabMasterMessages.NotFound;

                bool admin = IsAdminTier(callerTypeID);
                if (!admin && slab.OwnerUserID != callerUserID) return SlabDelegationPolicy.Message.NotYourSlab;
                if (!admin)
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), callerUserID);
                    if (!downline.Contains(mapping.UserID)) return SlabDelegationPolicy.Message.OutsideNetwork;
                }

                if (await _mappingRepo.Delete(userSlabMappingID) <= 0) return Messages.Common.Message.Failed;

                await Audit(new SlabCommissionAudit
                {
                    SlabMasterID = slab.SlabMasterID,
                    OwnerUserID = slab.OwnerUserID,
                    UpdatedByUserID = callerUserID,
                    Tier = SlabAuditTier.Unassign,
                    Remark = $"Removed \"{slab.SlabName}\" from user {mapping.UserID}. They now follow their upline's slab.",
                    IPAddress = ipAddress,
                });
                return StatusType.Ok;
            }
            catch (Exception ex) { _logger.Write(ex); return Messages.Common.Message.ExceptionOccured; }
        }

        private static string RoleName(int userTypeID) => userTypeID switch
        {
            UserTypeCode.Admin => "Administrator",
            UserTypeCode.SuperDistributor => "Super Distributor",
            UserTypeCode.Distributor => "Distributor",
            UserTypeCode.Retailer => "Retailer",
            UserTypeCode.ApiUser => "API User",
            UserTypeCode.SupportUser => "Support",
            UserTypeCode.Client => "Client",
            UserTypeCode.Accountant => "Accountant",
            _ => string.Empty,
        };

        // ── Keeping children honest when a parent card moves ────────────────────────────────

        public async Task<int> RevalidateChildren(int parentSlabMasterID, int byUserID, string ipAddress)
        {
            try
            {
                var allSlabs = await _slabMasterRepo.GetAll();
                var children = allSlabs.Where(s => !s.IsDeleted && s.ParentSlabID == parentSlabMasterID).ToList();
                if (children.Count == 0) return 0;

                var parentBands = (await _slabDetailRepo.GetBySlabMasterID(parentSlabMasterID))
                    .Where(b => !b.IsDeleted).ToList();

                int changed = 0;
                var audits = new List<SlabCommissionAudit>();

                foreach (var child in children)
                {
                    // A band the parent has and the child does not: clone it down, at the parent's
                    // own figures. Without this, a new band on the admin card would simply be
                    // MISSING from every reseller card, and the engine — which walks up when a card
                    // has no matching band — would quietly pay the retailer the admin's own
                    // (higher) rate on that amount range, bypassing every margin below it. Cloned
                    // at the parent's figures, so nothing changes commercially until the owner
                    // decides something; it just becomes a row they can decide about.
                    changed += await CloneMissingBands(parentBands, child);

                    var bands = (await _slabDetailRepo.GetBySlabMasterID(child.SlabMasterID))
                        .Where(b => !b.IsDeleted).ToList();

                    foreach (var band in bands)
                    {
                        // Measured against the parent CARD across this band's amount range, not
                        // against one paired row — the child may have split or re-typed the band,
                        // and a parent whose own bands have since moved still has to be able to
                        // say whether this one is inside what it allows.
                        var cands = SlabPayout.CandidateParents(parentBands, band);
                        string reason = SlabPayout.ValidateAgainstParent(
                            band, cands, band.FromAmount, band.ToAmount,
                            SlabDelegationPolicy.OwnTierOf(child.OwnerUserType));
                        var pb = SlabPayout.ParentAt(cands, band.FromAmount);

                        string want = string.IsNullOrEmpty(reason) ? SlabRowStatus.Active : SlabRowStatus.Invalid;
                        if (SlabRowStatus.Normalize(band.Status) == want) continue;

                        var updated = Clone(band);
                        updated.Status = want;
                        updated.UpdatedateTime = DateTime.UtcNow;
                        if (await _slabDetailRepo.Update(updated) <= 0) continue;
                        changed++;

                        audits.Add(new SlabCommissionAudit
                        {
                            SlabMasterID = child.SlabMasterID,
                            SlabDetailID = band.SlabDetailID,
                            OwnerUserID = child.OwnerUserID,
                            UpdatedByUserID = byUserID,
                            ServiceID = band.ServiceID,
                            OperatorID = band.OperatorID,
                            FromAmount = band.FromAmount,
                            ToAmount = band.ToAmount,
                            Tier = want == SlabRowStatus.Invalid ? SlabAuditTier.Invalidate : SlabAuditTier.Revalidate,
                            OldCommission = CommOf(band, SlabAuditTier.Retailer),
                            NewCommission = CommOf(band, SlabAuditTier.Retailer),
                            ParentCommission = pb == null ? 0 : pb.RetailerComm,
                            Remark = want == SlabRowStatus.Invalid
                                ? reason
                                : "Back inside the parent slab's limits.",
                            IPAddress = ipAddress,
                            DateTime = DateTime.UtcNow,
                        });
                    }

                    await RollUpStatus(child.SlabMasterID);
                    // A child card is itself a parent to the tier below it, so the re-check walks
                    // the whole subtree: admin lowering a rate has to reach a retailer card three
                    // levels down, not just the super-distributor's.
                    changed += await RevalidateChildren(child.SlabMasterID, byUserID, ipAddress);
                }

                await AuditRange(audits);
                return changed;
            }
            catch (Exception ex) { _logger.Write(ex); return 0; }
        }

        /// <summary>
        /// Roll the bands' states up onto the card, so a list screen can show "needs attention"
        /// without reading every detail row. The bands stay the authority — only the bands that
        /// are actually out of limit stop paying, which is what keeps one bad rate on one operator
        /// from stopping a whole network from trading.
        /// </summary>
        private async Task RollUpStatus(int slabMasterID)
        {
            var slab = await _slabMasterRepo.GetByID(slabMasterID);
            if (slab == null) return;

            var bands = await _slabDetailRepo.GetBySlabMasterID(slabMasterID);
            string want = bands.Any(b => !b.IsDeleted && SlabRowStatus.IsInvalid(b.Status))
                ? SlabRowStatus.Invalid : SlabRowStatus.Active;
            if (SlabRowStatus.Normalize(slab.Status) == want) return;

            slab.Status = want;
            slab.UpdatedateTime = DateTime.UtcNow;
            await _slabMasterRepo.Update(slab);
        }

        public async Task<(string failureMessage, int repairedBands)> FixToParent(
            int slabMasterID, int callerUserID, int callerTypeID, string ipAddress)
        {
            try
            {
                var slab = await _slabMasterRepo.GetByID(slabMasterID);
                if (slab == null) return (Messages.SlabMasterMessages.NotFound, 0);
                if (slab.OwnerUserID != callerUserID) return (SlabDelegationPolicy.Message.NotYourSlab, 0);
                if (slab.ParentSlabID is not int parentSlabID || parentSlabID <= 0)
                    return (SlabDelegationPolicy.Message.NoParentSlab, 0);

                string? ownTier = SlabDelegationPolicy.OwnTierOf(slab.OwnerUserType);
                var bands = (await _slabDetailRepo.GetBySlabMasterID(slabMasterID))
                    .Where(b => !b.IsDeleted && SlabRowStatus.IsInvalid(b.Status)).ToList();

                var caller = await _userRepo.GetUserById(callerUserID);
                var audits = new List<SlabCommissionAudit>();
                int repaired = 0;

                var allParentBands = (await _slabDetailRepo.GetBySlabMasterID(parentSlabID))
                    .Where(b => !b.IsDeleted).ToList();

                foreach (var band in bands)
                {
                    var cands = SlabPayout.CandidateParents(allParentBands, band);

                    // Pull the band back onto whichever parent band governs its start, range
                    // included. A band left straddling two parent rates, or sitting where the
                    // parent now prices nothing, has no single allowance to divide — so the range
                    // goes back to the parent's own, which is the one shape always repairable.
                    var fixedRow = Clone(band);
                    var pb = SlabPayout.ParentAt(cands, band.FromAmount)
                             ?? SlabPayout.ParentAt(cands, band.ToAmount);
                    if (pb == null) continue; // nothing to clamp to — the owner has to re-cut the card

                    fixedRow.FromAmount = pb.FromAmount;
                    fixedRow.ToAmount = pb.ToAmount;

                    // Put the band back to PASS-THROUGH: the owner's whole allowance on the one
                    // tier that was already getting the largest share of it, and nothing on the
                    // others. The owner keeps nothing until they re-type a margin.
                    //
                    // Pass-through is the one state guaranteed to be inside the limit whatever the
                    // parent changed to, and it lands exactly ON the allowance rather than
                    // somewhere arbitrary inside it. Clamping each tier to the parent's own column
                    // (what this did before) compared the owner's outlay against the parent's
                    // costs, which are different money.
                    var settableTiers = SlabDelegationPolicy.TiersSettableBy(slab.OwnerUserType);
                    string keepTier = settableTiers
                        .OrderByDescending(t => Math.Abs(SignedOf(band, t)))
                        .FirstOrDefault() ?? SlabDelegationPolicy.DefaultPassThroughTier(slab.OwnerUserType);
                    if (Math.Abs(SignedOf(band, keepTier)) == 0m)
                        keepTier = SlabDelegationPolicy.DefaultPassThroughTier(slab.OwnerUserType);

                    foreach (var tier in settableTiers)
                    {
                        SetComm(fixedRow, tier, 0m);
                        SetCommType(fixedRow, tier, CommTypeFix);
                        SetCategory(fixedRow, tier, CategoryCommission);
                    }

                    var repairAllowance = SlabPayout.AllowanceCurve(pb, ownTier);
                    bool repairPercent = repairAllowance.Percent != 0m;
                    decimal repairValue = repairPercent ? repairAllowance.Percent : repairAllowance.Flat;
                    SetComm(fixedRow, keepTier, Math.Abs(repairValue));
                    SetCommType(fixedRow, keepTier, repairPercent ? CommTypeFlexi : CommTypeFix);
                    SetCategory(fixedRow, keepTier, repairValue < 0 ? CategorySurcharge : CategoryCommission);

                    fixedRow.GSTType = pb.GSTType;
                    fixedRow.GSTRatePercent = pb.GSTRatePercent;

                    fixedRow.ParentSlabDetailID = pb.SlabDetailID;
                    fixedRow.ParentRetailerComm = pb.RetailerComm;
                    fixedRow.ParentDistributorComm = pb.DistributorComm;
                    fixedRow.ParentSuperDistributorComm = pb.SuperDistributorComm;
                    fixedRow.ParentClientComm = pb.ClientComm;
                    fixedRow.ParentCommType = pb.RetailerCommType;
                    fixedRow.ParentGSTType = pb.GSTType;
                    fixedRow.ParentGSTRatePercent = pb.GSTRatePercent;

                    ClearOwnTier(fixedRow, slab.OwnerUserType);

                    // If the repair still does not satisfy the ceiling, leave the band invalid
                    // rather than writing something that only looks repaired.
                    if (!string.IsNullOrEmpty(SlabPayout.ValidateAgainstParent(
                            fixedRow, cands, fixedRow.FromAmount, fixedRow.ToAmount, ownTier))) continue;

                    fixedRow.Status = SlabRowStatus.Active;
                    fixedRow.UpdatedateTime = DateTime.UtcNow;
                    if (await _slabDetailRepo.Update(fixedRow) <= 0) continue;
                    repaired++;

                    foreach (var tier in Tiers)
                    {
                        decimal before = CommOf(band, tier);
                        decimal after = CommOf(fixedRow, tier);
                        if (before == after) continue;
                        audits.Add(new SlabCommissionAudit
                        {
                            SlabMasterID = slab.SlabMasterID,
                            SlabDetailID = band.SlabDetailID,
                            OwnerUserID = slab.OwnerUserID,
                            ParentUserID = caller?.ParentID,
                            UpdatedByUserID = callerUserID,
                            ServiceID = band.ServiceID,
                            OperatorID = band.OperatorID,
                            FromAmount = band.FromAmount,
                            ToAmount = band.ToAmount,
                            Tier = tier,
                            OldCommission = before,
                            NewCommission = after,
                            OldCommType = TypeOf(band, tier),
                            NewCommType = TypeOf(fixedRow, tier),
                            ParentCommission = CommOf(pb, tier),
                            Remark = "Brought back inside the parent slab's limits.",
                            IPAddress = ipAddress,
                            DateTime = DateTime.UtcNow,
                        });
                    }
                }

                await AuditRange(audits);
                await RollUpStatus(slabMasterID);
                await RevalidateChildren(slabMasterID, callerUserID, ipAddress);
                return (StatusType.Ok, repaired);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, 0); }
        }

        // ── Audit ───────────────────────────────────────────────────────────────────────────

        private async Task Audit(SlabCommissionAudit row)
        {
            if (row.DateTime == default) row.DateTime = DateTime.UtcNow;
            await AuditRange(new[] { row });
        }

        /// <summary>
        /// Append audit rows, never failing the caller. A rate change that saved and whose audit row
        /// did not is bad; a rate change REFUSED because the audit table was briefly unavailable is
        /// worse, and the log is reconstructible from the rows themselves.
        /// </summary>
        private async Task AuditRange(IEnumerable<SlabCommissionAudit> rows)
        {
            var list = rows.ToList();
            if (list.Count == 0) return;
            foreach (var r in list) if (r.DateTime == default) r.DateTime = DateTime.UtcNow;
            try { await _auditRepo.InsertRange(list); }
            catch (Exception ex) { _logger.Write(ex); }
        }

        public async Task<(string failureMessage, SlabAuditWrapper data)> Audit(
            SlabAuditRequest request, int callerUserID, int callerTypeID)
        {
            try
            {
                bool admin = IsAdminTier(callerTypeID);
                IEnumerable<int>? ownerScope = null;
                if (!admin)
                {
                    // Own slabs plus every slab owned by somebody beneath them: a
                    // super-distributor answering for its branch has to be able to see what its
                    // distributors did to their retailers' rates.
                    var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), callerUserID);
                    downline.Add(callerUserID);
                    ownerScope = downline;
                }

                int pageSize = request.PageSize > 0 ? Math.Min(request.PageSize, 500) : 25;
                int skip = Math.Max(request.PageNumber - 1, 0) * pageSize;

                var (count, rows) = await _auditRepo.Search(
                    request.SlabMasterID, request.ServiceID, request.FromDate, request.ToDate,
                    ownerScope, skip, pageSize);

                var slabs = await _slabMasterRepo.GetAll();
                var users = await _userRepo.GetAllUsersWithDeleted();
                var services = await _serviceRepo.GetAllServices();
                var operators = await _operatorRepo.GetAllOperators();
                string NameOf(int? id) => id is int i && i > 0
                    ? users.FirstOrDefault(u => u.UserMasterID == i) is UserMaster u
                        ? (!string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : u.PersonName ?? $"User {i}")
                        : $"User {i}"
                    : "Admin";

                var wrapper = new SlabAuditWrapper
                {
                    TotalCount = count,
                    Rows = rows.Select(r => new SlabAuditRow
                    {
                        SlabCommissionAuditID = r.SlabCommissionAuditID,
                        SlabMasterID = r.SlabMasterID,
                        SlabName = slabs.FirstOrDefault(s => s.SlabMasterID == r.SlabMasterID)?.SlabName ?? string.Empty,
                        SlabDetailID = r.SlabDetailID,
                        OwnerUserID = r.OwnerUserID,
                        OwnerName = NameOf(r.OwnerUserID),
                        ParentUserID = r.ParentUserID,
                        ParentName = NameOf(r.ParentUserID),
                        UpdatedByUserID = r.UpdatedByUserID,
                        UpdatedByName = NameOf(r.UpdatedByUserID),
                        ServiceID = r.ServiceID,
                        ServiceName = services.FirstOrDefault(s => s.ServiceID == r.ServiceID)?.Name ?? string.Empty,
                        OperatorID = r.OperatorID,
                        OperatorName = operators.FirstOrDefault(o => o.OperatorID == r.OperatorID)?.OperatorName ?? string.Empty,
                        FromAmount = r.FromAmount,
                        ToAmount = r.ToAmount,
                        Tier = r.Tier,
                        OldCommission = r.OldCommission,
                        NewCommission = r.NewCommission,
                        OldCommType = r.OldCommType,
                        NewCommType = r.NewCommType,
                        ParentCommission = r.ParentCommission,
                        Remark = r.Remark,
                        IPAddress = r.IPAddress,
                        DateTime = r.DateTime,
                    }).ToList(),
                };

                return (StatusType.Ok, wrapper);
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Common.Message.ExceptionOccured, new SlabAuditWrapper()); }
        }
    }
}
