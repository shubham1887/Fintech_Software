using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.CommissionTax;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IChargeGstResolver" />
    ///
    /// The GST on a charge is CONFIGURED, in four layers, and this is the only place in the product
    /// that applies the formula. Before this, a charge's tax treatment could only be stated per
    /// provider or per service, so a rate card with one exempt band — or a slab reading
    /// "500-1000 inclusive, 1001-5000 exclusive, 5001-25000 no GST" — could not be expressed at
    /// all. The provider side was never split either: the GST inside a DMT charge, which is input
    /// credit, was recorded nowhere.
    public class ChargeGstResolver : IChargeGstResolver
    {
        private readonly IProviderConfigurationAdminCommissionService _providerBands;
        private readonly ISlabCommissionResolver _slabs;
        private readonly ICommissionTaxResolver _profiles;
        private readonly IUserSlabMappingRepository _slabMappings;
        private readonly Logger _logger;

        public ChargeGstResolver(
            IProviderConfigurationAdminCommissionService providerBands,
            ISlabCommissionResolver slabs,
            ICommissionTaxResolver profiles,
            IUserSlabMappingRepository slabMappings)
        {
            _providerBands = providerBands;
            _slabs = slabs;
            _profiles = profiles;
            _slabMappings = slabMappings;
            _logger = new Logger();
        }

        // ── The formula. The only copy of it. ─────────────────────────────────────────────────

        /// <summary>
        /// Split one charge under one decision.
        ///
        ///   INCLUSIVE  GST = charge x rate / (100 + rate);  base = charge - GST;  total = charge
        ///              5.90 at 18% -> 0.90 GST, 5.00 base, 5.90 payable.
        ///   EXCLUSIVE  GST = charge x rate / 100;           base = charge;        total = charge + GST
        ///              5.00 at 18% -> 0.90 GST, 5.00 base, 5.90 payable.
        ///   NONE       GST = 0;                             base = total = charge.
        ///
        /// The sign of the charge is preserved throughout: a surcharge the user pays, and a
        /// provider charge such as DMT, are negative commissions in this system, so the GST and
        /// base of a negative charge are negative too and a report can sum a mixed set.
        ///
        /// A rate of 0 splits nothing out whatever the type says — there is no tax at 0% — and the
        /// callers attach a warning when that happens, because it means a rate could not be
        /// resolved rather than that the charge is exempt.
        /// </summary>
        public static ChargeGstBreakdown Split(decimal quotedCharge, string gstType, decimal ratePercent)
        {
            string type = ChargeGstType.Normalize(gstType);
            if (type == ChargeGstType.Default) type = ChargeGstType.None;

            var result = new ChargeGstBreakdown
            {
                ChargeAsQuoted = R2(quotedCharge),
                GSTType = type,
                GSTRatePercent = ratePercent,
            };

            decimal magnitude = Math.Abs(quotedCharge);
            decimal sign = quotedCharge < 0m ? -1m : 1m;

            if (magnitude == 0m || type == ChargeGstType.None || ratePercent <= 0m)
            {
                result.GSTAmount = 0m;
                result.BaseCharge = R2(quotedCharge);
                result.TotalCharge = R2(quotedCharge);
                return result;
            }

            if (type == ChargeGstType.Inclusive)
            {
                decimal gst = R2(magnitude * ratePercent / (100m + ratePercent));
                result.GSTAmount = sign * gst;
                result.BaseCharge = sign * R2(magnitude - gst);
                result.TotalCharge = R2(quotedCharge);
                return result;
            }

            decimal added = R2(magnitude * ratePercent / 100m);
            result.GSTAmount = sign * added;
            result.BaseCharge = R2(quotedCharge);
            result.TotalCharge = sign * R2(magnitude + added);
            return result;
        }

        // ── Provider side ────────────────────────────────────────────────────────────────────

        public async Task<ChargeGstBreakdown> ForProviderCharge(
            int providerID, int serviceID, int operatorID, decimal amount, decimal quotedCharge, DateTime asOf)
        {
            try
            {
                var band = await _providerBands.ResolveBand(providerID, serviceID, operatorID, amount);

                // A PROVIDER-scoped profile is about money moving between us and that provider, so
                // the provider is named here and the user's slab deliberately is not.
                var treatment = await _profiles.Resolve(serviceID, operatorID, providerID, slabMasterId: 0, asOf);

                string bandLabel = band == null
                    ? "no provider band"
                    : $"provider band {Money(band.FromAmount)}-{Money(band.ToAmount)}";

                var decision = Decide(band?.GSTType, band?.GSTRatePercent ?? 0m, treatment, bandLabel);
                var split = Split(quotedCharge, decision.GSTType, decision.Rate);

                split.ChargeType = band?.ChargeType ?? string.Empty;
                split.DecidedBy = decision.DecidedBy;
                split.Warning = Warn(decision, split);
                split.BandMatched = band != null;
                split.BandID = band?.BandID ?? 0;
                split.BandFromAmount = band?.FromAmount ?? 0m;
                split.BandToAmount = band?.ToAmount ?? 0m;
                return split;
            }
            catch (Exception ex)
            {
                // Never take a settlement down over a tax split. An unresolved breakdown is
                // reported as unresolved; a thrown one would strand money mid-flight.
                _logger.Write(ex);
                return Unresolved(quotedCharge);
            }
        }

        // ── Downline side ────────────────────────────────────────────────────────────────────

        public async Task<SlabSideGst> ForSlabSide(
            int userID, int serviceID, int operatorID, decimal amount,
            decimal retailerQuoted, decimal distributorQuoted, decimal superDistributorQuoted, DateTime asOf)
        {
            var side = new SlabSideGst();
            try
            {
                var slab = await _slabs.Resolve(userID, serviceID, operatorID, amount);

                // The band may not have matched — a tier paid from a route default, a service with
                // no slab at all — and the profile still has to be found, so fall back to whichever
                // slab the user is mapped to.
                int slabMasterID = slab.SlabMasterID > 0 ? slab.SlabMasterID : await MappedSlabID(userID);

                // ProviderID is deliberately 0: a PROVIDER-scoped profile describes what a provider
                // pays US, and must not decide how we tax our own retailer.
                var treatment = await _profiles.Resolve(serviceID, operatorID, providerId: 0, slabMasterID, asOf);

                string bandLabel = slab.SlabFound
                    ? $"slab band {Money(slab.FromAmount)}-{Money(slab.ToAmount)}"
                      + (string.IsNullOrWhiteSpace(slab.SlabName) ? string.Empty : $" on {slab.SlabName}")
                    : "no slab band";

                var decision = Decide(slab.SlabFound ? slab.GSTType : null, slab.GSTRatePercent, treatment, bandLabel);

                side.Retailer = Tier(retailerQuoted, decision, slab.RetailerChargeType, slab);
                side.Distributor = Tier(distributorQuoted, decision, slab.DistributorChargeType, slab);
                side.SuperDistributor = Tier(superDistributorQuoted, decision, slab.SuperDistributorChargeType, slab);

                side.SlabMasterID = slabMasterID;
                side.SlabDetailID = slab.SlabDetailID;
                side.SlabName = slab.SlabName;
                side.BandMatched = slab.SlabFound;
                return side;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                side.Retailer = Unresolved(retailerQuoted);
                side.Distributor = Unresolved(distributorQuoted);
                side.SuperDistributor = Unresolved(superDistributorQuoted);
                return side;
            }
        }

        // ── Preview ──────────────────────────────────────────────────────────────────────────

        public async Task<ChargeGstPreview> Preview(
            int userID, int providerID, int serviceID, int operatorID, decimal amount, DateTime asOf)
        {
            var preview = new ChargeGstPreview { Amount = amount };

            var band = await _providerBands.ResolveBand(providerID, serviceID, operatorID, amount);
            decimal providerQuoted = band?.SignedCommission ?? 0m;
            preview.Provider = await ForProviderCharge(providerID, serviceID, operatorID, amount, providerQuoted, asOf);

            var slab = await _slabs.Resolve(userID, serviceID, operatorID, amount);
            var side = await ForSlabSide(userID, serviceID, operatorID, amount,
                slab.RetailerComm, slab.DistributorComm, slab.SuperDistributorComm, asOf);

            preview.Retailer = side.Retailer;
            preview.Distributor = side.Distributor;
            preview.SuperDistributor = side.SuperDistributor;
            preview.SlabName = side.SlabName;
            preview.SlabMasterID = side.SlabMasterID;
            preview.SlabDetailID = side.SlabDetailID;

            decimal paidAway = preview.Retailer.BaseCharge + preview.Distributor.BaseCharge
                             + preview.SuperDistributor.BaseCharge;
            preview.NetProfit = R2(preview.Provider.BaseCharge - paidAway);

            preview.Notes.Add(band == null
                ? "No provider band matches this amount, so the provider leg previews as 0. The route's own flat commission, if it has one, is applied at dispatch."
                : $"Provider band {Money(band.FromAmount)}-{Money(band.ToAmount)} charges {Money(Math.Abs(band.SignedCommission))} ({band.ChargeType.ToLowerInvariant()}). {preview.Provider.DecidedBy}.");

            preview.Notes.Add(!slab.SlabFound
                ? "No slab band matches this amount for this user, so no downline commission is payable."
                : $"Slab band {Money(slab.FromAmount)}-{Money(slab.ToAmount)}"
                  + (string.IsNullOrWhiteSpace(side.SlabName) ? string.Empty : $" on {side.SlabName}")
                  + $" pays the retailer {Money(slab.RetailerComm)}. {preview.Retailer.DecidedBy}.");

            preview.Notes.Add($"Net to the house: {Money(preview.Provider.BaseCharge)} earned net of GST,"
                + $" less {Money(paidAway)} paid away net of GST, is {Money(preview.NetProfit)}."
                + " GST is not profit either way: it is collected and paid on.");

            if (!string.IsNullOrWhiteSpace(preview.Provider.Warning)) preview.Notes.Add(preview.Provider.Warning);
            if (!string.IsNullOrWhiteSpace(preview.Retailer.Warning)) preview.Notes.Add(preview.Retailer.Warning);

            return preview;
        }

        // ── Deciding which layer wins ────────────────────────────────────────────────────────

        private sealed record GstDecision(string GSTType, decimal Rate, string DecidedBy, string ProfileWarning, bool FromBand);

        /// <summary>
        /// The band decides when it says anything at all; otherwise the profile does. A band set to
        /// DEFAULT — which is every row that existed before the column — changes nothing, which is
        /// why this ships onto a live install without moving a single figure.
        /// </summary>
        private static GstDecision Decide(string? bandGstType, decimal bandRate, CommissionTaxTreatment treatment, string bandLabel)
        {
            string onBand = ChargeGstType.Normalize(bandGstType);

            if (onBand != ChargeGstType.Default)
            {
                decimal rate = bandRate > 0m ? bandRate : treatment.GSTRatePercent;
                string rateFrom = bandRate > 0m
                    ? $"{Pct(rate)} pinned on the band"
                    : treatment.RateFromTaxEngine
                        ? $"{Pct(rate)} from the tax engine"
                        : $"{Pct(rate)} from the {Lower(treatment.Scope)} profile";

                return new GstDecision(onBand, onBand == ChargeGstType.None ? 0m : rate,
                    $"{bandLabel} is set to {Word(onBand)}, {rateFrom}", treatment.Warning, FromBand: true);
            }

            // Nothing on the row: the configured profile decides, exactly as it did before.
            if (string.Equals(treatment.TaxBase, CommissionTaxBase.Gross, StringComparison.OrdinalIgnoreCase))
            {
                // The profile says the tax follows the whole throughput — talktime bought and
                // resold — so the GST is on the amount and is already stamped on the transaction's
                // tax detail. Splitting the commission as well would count the same tax twice.
                return new GstDecision(ChargeGstType.None, 0m,
                    $"{bandLabel} leaves it to the {Lower(treatment.Scope)} profile ({treatment.ScopeLabel}), "
                    + "which taxes the GROSS amount rather than the charge - that tax is on the transaction's tax stamp, not here",
                    treatment.Warning, FromBand: false);
            }

            string mode = ChargeGstType.Normalize(treatment.TaxMode);
            if (mode == ChargeGstType.Default) mode = ChargeGstType.Inclusive;

            string source = treatment.RateFromTaxEngine ? "from the tax engine" : "pinned on the profile";
            return new GstDecision(mode, mode == ChargeGstType.None ? 0m : treatment.GSTRatePercent,
                $"{bandLabel} leaves it to the {Lower(treatment.Scope)} profile ({treatment.ScopeLabel}), "
                + $"which is {Word(mode)} at {Pct(treatment.GSTRatePercent)} {source}",
                treatment.Warning, FromBand: false);
        }

        private static ChargeGstBreakdown Tier(
            decimal quoted, GstDecision decision, string chargeType, ResolvedCommission slab)
        {
            var split = Split(quoted, decision.GSTType, decision.Rate);
            split.ChargeType = chargeType;
            split.DecidedBy = decision.DecidedBy;
            split.Warning = Warn(decision, split);
            split.BandMatched = slab.SlabFound;
            split.BandID = slab.SlabDetailID;
            split.BandFromAmount = slab.FromAmount;
            split.BandToAmount = slab.ToAmount;
            return split;
        }

        /// <summary>
        /// What the caller should be told, in order of consequence. A 0% rate on a charge that is
        /// supposed to carry tax is the one worth shouting about: it silently books the tax as
        /// income, which is the mistake this whole mechanism exists to prevent.
        /// </summary>
        private static string Warn(GstDecision decision, ChargeGstBreakdown split)
        {
            if (split.ChargeAsQuoted != 0m && decision.GSTType != ChargeGstType.None && decision.Rate <= 0m)
            {
                return "The GST rate resolved to 0%, so nothing was split out of this charge. Either pin a rate on "
                     + "the band, or set the service's rate on the tax engine. Until then the whole charge is booked "
                     + "as income.";
            }

            return string.IsNullOrWhiteSpace(decision.ProfileWarning) ? string.Empty : decision.ProfileWarning;
        }

        private static ChargeGstBreakdown Unresolved(decimal quoted) => new()
        {
            ChargeAsQuoted = R2(quoted),
            GSTType = string.Empty,
            BaseCharge = R2(quoted),
            TotalCharge = R2(quoted),
            DecidedBy = "unresolved",
            Warning = "The GST on this charge could not be resolved and nothing was split out of it. "
                    + "The figure stands as quoted; see the API log.",
        };

        private async Task<int> MappedSlabID(int userID)
        {
            var mappings = await _slabMappings.GetByUserID(userID);
            return mappings.Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.UserSlabMappingID)
                .Select(m => m.SlabMasterID)
                .FirstOrDefault();
        }

        private static string Word(string gstType) => gstType switch
        {
            ChargeGstType.Inclusive => "inclusive of GST",
            ChargeGstType.Exclusive => "exclusive of GST",
            ChargeGstType.None => "no GST",
            _ => Lower(gstType),
        };

        private static string Lower(string? s) => (s ?? string.Empty).ToLowerInvariant();
        private static string Pct(decimal rate) => $"{rate:0.##}%";
        private static string Money(decimal v) => $"Rs {v:0.00}";
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
