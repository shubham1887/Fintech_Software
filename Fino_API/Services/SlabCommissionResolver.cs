using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from the legacy SlabDetailBAL commission path (GetCommission + GetLower +
    // GetUplineDetailsWithCommission). Resolution order for the slab:
    //   1. the user's own UserSlabMapping
    //   2. then each ancestor's mapping in turn (uplines share their slab downwards)
    // The matching SlabDetail row is service + operator + amount-range scoped. Each tier value is
    // then normalised: a percentage type = percent of amount, a flat type = flat; the SURCHARGE
    // category flips the sign so the pipeline can treat "commission" and "surcharge" as one signed
    // number.
    //
    // Upline commissions are payable only to uplines that actually exist in the chain
    // (Case 1: Admin -> SD -> Dist -> Retailer pays all tiers; Case 2: Admin -> Retailer pays none
    // of the upline tiers - the difference implicitly stays with admin in AdminCommission).
    public class SlabCommissionResolver : ISlabCommissionResolver
    {
        // Legacy guards against a fat-fingered slab moving absurd money. Applied PER TIER: a bad
        // distributor value must not silently void the retailer's (and everyone else's) commission.
        private const decimal MaxRetailerCommission = 3000m;
        private const decimal MaxDistributorCommission = 1000m;
        private const decimal MaxSuperDistributorCommission = 900m;

        // How far up the parent chain we look for a slab mapping. Retailer -> Distributor ->
        // Super-Distributor -> Admin is already 4 levels, so the old depth of 3 was too shallow to
        // reach a slab that is only mapped high up in the network.
        private const int MaxChainWalk = 8;

        private readonly IUserSlabMappingRepository _mappingRepo;
        private readonly ISlabDetailRepository _slabDetailRepo;
        private readonly ISlabMasterRepository _slabMasterRepo;
        private readonly IUserRepository _userRepo;
        private readonly Logger _logger;

        public SlabCommissionResolver(
            IUserSlabMappingRepository mappingRepo,
            ISlabDetailRepository slabDetailRepo,
            ISlabMasterRepository slabMasterRepo,
            IUserRepository userRepo)
        {
            _mappingRepo = mappingRepo;
            _slabDetailRepo = slabDetailRepo;
            _slabMasterRepo = slabMasterRepo;
            _userRepo = userRepo;
            _logger = new Logger();
        }

        /// <summary>One level's rate: the band that priced it, and who funds it (0 = admin).</summary>
        private readonly record struct TierRate(SlabDetail? Band, int FunderID);

        public async Task<ResolvedCommission> Resolve(int userID, int serviceID, int operatorID, decimal amount)
        {
            var result = new ResolvedCommission();
            try
            {
                var (slab, ownerUserID) = await FindSlabDetail(userID, serviceID, operatorID, amount);
                if (slab == null || !slab.IsActive) return result;

                // A delegated band whose parent card was lowered beneath it is not payable. Refuse
                // rather than walk on up: the next card up is the one that pays MORE, and quietly
                // paying it is the overpayment the invalid state is there to stop.
                if (Entities.SlabRowStatus.IsInvalid(slab.Status))
                {
                    result.SlabMasterID = slab.SlabMasterID;
                    result.SlabDetailID = slab.SlabDetailID;
                    result.SlabName = slab.SlabMaster?.SlabName ?? string.Empty;
                    result.BlockedReason = SlabDelegationPolicy.Message.ParentUpdated;
                    return result;
                }

                var user = await _userRepo.GetUserById(userID);

                // The transacting user always earns the "retailer" tier - except a Client-type
                // account, which earns the client tier instead.
                decimal retailer = user?.UserTypeID == UserTypeCode.Client
                    ? Normalize(slab.ClientComm, slab.ClientCommType, amount, slab.ClientCommissionCategory)
                    : Normalize(slab.RetailerComm, slab.RetailerCommType, amount, slab.RetailerCommissionCategory);

                // Each upline's rate is read off the card assigned to THAT upline, not off the
                // transacting user's card. On a non-delegated install both lookups land on the same
                // card and the figures are the ones this always produced; once a distributor writes
                // its own retailer card they part company, which is the point — the distributor's
                // ₹12 comes from admin's card and the retailer's ₹10 comes from the distributor's,
                // so the ₹2 margin is the gap between two postings instead of a third column.
                var (distributorID, superDistributorID) = await FindUplines(user);

                var distRate = await RateFor(distributorID, serviceID, operatorID, amount, ownerUserID, slab);
                var sdRate = await RateFor(superDistributorID, serviceID, operatorID, amount, ownerUserID, slab);

                decimal distributor = distRate.Band == null ? 0m : Normalize(
                    distRate.Band.DistributorComm, distRate.Band.DistributorCommType, amount,
                    distRate.Band.DistributorCommissionCategory);
                decimal superDist = sdRate.Band == null ? 0m : Normalize(
                    sdRate.Band.SuperDistributorComm, sdRate.Band.SuperDistributorCommType, amount,
                    sdRate.Band.SuperDistributorCommissionCategory);

                // Sanity guard, per tier: drop only the tier that is out of range, so one bad value
                // no longer voids the whole row (the old guard returned nothing at all).
                if (Math.Abs(retailer) >= MaxRetailerCommission) retailer = 0m;
                if (Math.Abs(distributor) >= MaxDistributorCommission) distributor = 0m;
                if (Math.Abs(superDist) >= MaxSuperDistributorCommission) superDist = 0m;

                result.SlabFound = true;

                // Which row decided, carried out with the figures. The GST decision now lives on
                // the band, and matching the same band again downstream could answer differently
                // if the card was edited in between.
                result.SlabMasterID = slab.SlabMasterID;
                result.SlabDetailID = slab.SlabDetailID;
                result.SlabName = slab.SlabMaster?.SlabName ?? string.Empty;
                result.FromAmount = slab.FromAmount;
                result.ToAmount = slab.ToAmount;
                result.GSTType = ChargeGstType.Normalize(slab.GSTType);
                result.GSTRatePercent = slab.GSTRatePercent;
                result.RetailerChargeType = ChargeTypeOf(user?.UserTypeID == UserTypeCode.Client
                    ? slab.ClientCommType : slab.RetailerCommType);
                result.DistributorChargeType = ChargeTypeOf(distRate.Band?.DistributorCommType);
                result.SuperDistributorChargeType = ChargeTypeOf(sdRate.Band?.SuperDistributorCommType);
                result.RetailerComm = retailer;
                result.RetailerFunderID = ownerUserID;

                // A tier the chain does not have is simply not paid, and the difference implicitly
                // stays with admin (Case 2: Admin -> Retailer direct).
                if (distributorID > 0)
                {
                    result.DistributorUserID = distributorID;
                    result.DistributorComm = distributor;
                    result.DistributorFunderID = distRate.FunderID;
                }
                if (superDistributorID > 0)
                {
                    result.SuperDistributorUserID = superDistributorID;
                    result.SuperDistributorComm = superDist;
                    result.SuperDistributorFunderID = sdRate.FunderID;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return new ResolvedCommission();
            }
        }

        // Nearest Distributor and nearest Super-Distributor above the transacting user. Walking the
        // chain (rather than only inspecting parent + grandparent) means an extra tier in between,
        // or a retailer parked directly under a super-distributor, still pays the right people.
        private async Task<(int distributorID, int superDistributorID)> FindUplines(UserMaster? user)
        {
            int distributorID = 0, superDistributorID = 0;
            int probeID = user?.ParentID ?? 0;

            for (int depth = 0; depth < MaxChainWalk && probeID > 0; depth++)
            {
                var ancestor = await _userRepo.GetUserById(probeID);
                if (ancestor == null) break;
                if (ancestor.UserTypeID == UserTypeCode.Admin) break;

                if (distributorID == 0 && ancestor.UserTypeID == UserTypeCode.Distributor)
                    distributorID = ancestor.UserMasterID;
                else if (superDistributorID == 0 && ancestor.UserTypeID == UserTypeCode.SuperDistributor)
                    superDistributorID = ancestor.UserMasterID;

                if (distributorID > 0 && superDistributorID > 0) break;
                probeID = ancestor.ParentID ?? 0;
            }

            return (distributorID, superDistributorID);
        }

        // The user's own slab first, then each ancestor's in turn (legacy walk, deepened). Returns
        // the band AND who owns the card it came from — 0 for an admin card, which is every card
        // on an install that does not delegate. The owner is who funds whatever that band pays.
        private async Task<(SlabDetail? row, int ownerUserID)> FindSlabDetail(
            int userID, int serviceID, int operatorID, decimal amount)
        {
            int probeUserID = userID;
            for (int depth = 0; depth < MaxChainWalk && probeUserID > 0; depth++)
            {
                // Newest mapping wins when a user was re-assigned to another slab.
                var mapping = (await _mappingRepo.GetByUserID(probeUserID))
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.UserSlabMappingID)
                    .FirstOrDefault();
                if (mapping != null)
                {
                    var row = await MatchSlabRow(mapping.SlabMasterID, serviceID, operatorID, amount);
                    if (row != null) return (row, await OwnerOf(mapping.SlabMasterID));
                }
                var user = await _userRepo.GetUserById(probeUserID);
                probeUserID = user?.ParentID ?? 0;
            }
            return (null, 0);
        }

        /// <summary>
        /// Who manages a card. 0 for an admin card (OwnerUserID null), which is both the default
        /// and what every row on a non-delegated install reads as — so the posting code's "0 means
        /// the admin funds it" is the unchanged behaviour, not a special case.
        /// </summary>
        private async Task<int> OwnerOf(int slabMasterID)
        {
            var master = await _slabMasterRepo.GetByID(slabMasterID);
            return master?.OwnerUserID ?? 0;
        }

        /// <summary>
        /// One upline's own rate: the band on the card assigned to THEM, and who funds it.
        ///
        /// Falls back to the transacting user's band when the upline holds no card of their own and
        /// none of their uplines do either — which is how this behaved before per-level lookup, and
        /// keeps a sparsely-mapped network paying what it has always paid.
        ///
        /// An upline band marked INVALID (its own card hands out more than the card above allows)
        /// pays that tier nothing. Paying it would hand out money the card above does not cover;
        /// refusing the whole transaction would stop a retailer trading over a rate dispute two
        /// levels above them, which is worse. The invalid band is already flagged on its owner's
        /// own screen and in the audit trail.
        /// </summary>
        private async Task<TierRate> RateFor(int uplineUserID, int serviceID, int operatorID,
            decimal amount, int fallbackOwnerID, SlabDetail fallbackBand)
        {
            if (uplineUserID <= 0) return new TierRate(null, 0);

            var (row, ownerID) = await FindSlabDetail(uplineUserID, serviceID, operatorID, amount);
            if (row == null || !row.IsActive) return new TierRate(fallbackBand, fallbackOwnerID);
            if (Entities.SlabRowStatus.IsInvalid(row.Status)) return new TierRate(null, 0);
            return new TierRate(row, ownerID);
        }

        // ServiceID/OperatorID of 0 on a slab row means "any" (legacy wildcard). When several rows
        // can serve the amount, the most specific one wins: exact operator before wildcard, exact
        // service before wildcard, then the narrowest amount band.
        private async Task<SlabDetail?> MatchSlabRow(int slabMasterID, int serviceID, int operatorID, decimal amount)
        {
            var rows = await _slabDetailRepo.GetBySlabMasterID(slabMasterID);
            return rows
                .Where(r => r.IsActive && !r.IsDeleted &&
                            (r.OperatorID == operatorID || r.OperatorID == 0) &&
                            (r.ServiceID == serviceID || r.ServiceID == 0) &&
                            amount >= r.FromAmount && amount <= r.ToAmount)
                .OrderBy(r => r.OperatorID == operatorID ? 0 : 1)
                .ThenBy(r => r.ServiceID == serviceID ? 0 : 1)
                .ThenBy(r => r.ToAmount - r.FromAmount)
                .ThenBy(r => r.SlabDetailID)
                .FirstOrDefault();
        }

        // -- Slab vocabulary -------------------------------------------------------
        // CommType and CommissionCategory are free-text columns and the screens that write them have
        // not agreed on a spelling: the provider commission editor writes "Flexi"/"Fix" and
        // "Commission"/"Surcharge", the slab commission editor used to write "Percentage"/"Flat" and
        // "COM"/"SUR". Neither of the latter matched, so every percentage row was applied as a flat
        // amount and every surcharge row was paid out as a commission. Recognise all spellings so an
        // existing row keeps working whichever screen wrote it.
        private static readonly string[] PercentTypes = { "FLEXI", "PERCENT", "PERCENTAGE", "PER", "%", "P" };
        private static readonly string[] SurchargeCategories = { "SURCHARGE", "SUR", "CHARGE", "S" };

        public static bool IsPercentType(string? commType)
            => PercentTypes.Contains((commType ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase);

        public static bool IsSurchargeCategory(string? category)
            => SurchargeCategories.Contains((category ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The canonical word for what a free-text CommType column means. Three screens have
        /// written three spellings of the same two ideas; a settled transaction stamps one word so
        /// a report never has to know which screen wrote the row.
        /// </summary>
        public static string ChargeTypeOf(string? commType)
            => IsPercentType(commType) ? ChargeBasis.Percentage : ChargeBasis.Flat;

        // Legacy GetLower: SURCHARGE flips the sign, a percentage type means percent-of-amount, a
        // flat type is used as-is.
        public static decimal Normalize(decimal comm, string commType, decimal amount, string category)
        {
            if (comm == 0) return 0;

            // A negative value is a surcharge however the category column is spelled.
            bool surcharge = comm < 0 || IsSurchargeCategory(category);

            decimal value = Math.Abs(comm);
            if (IsPercentType(commType))
                value = amount * (value / 100m);

            return surcharge ? value * -1 : value;
        }
    }
}
