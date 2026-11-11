using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PartyState;
using FintechApp.Responses.PartyState;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IPartyStateService" />
    public class PartyStateService : IPartyStateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPartyStateResolver _resolver;
        private readonly Logger _logger = new();

        public PartyStateService(ApplicationDbContext context, IPartyStateResolver resolver)
        {
            _context = context;
            _resolver = resolver;
        }

        public async Task<PartyStateReport> Review()
        {
            var users = await _context.UserMasters.AsNoTracking()
                .Where(u => !u.IsDeleted)
                .Select(u => new
                {
                    u.UserMasterID, u.UserTypeID, u.FirmName, u.PersonName, u.MobileNumber,
                    u.GSTNo, u.BusinessGSTStateCode, u.StateID,
                })
                .ToListAsync();

            var profiles = await _context.PartyTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync();
            var profileByUser = profiles
                .GroupBy(p => p.UserID)
                .ToDictionary(g => g.Key, g => g.First());

            // How many documents each party already has. A party with documents cannot simply be
            // corrected — the documents keep the code they were stamped with.
            var docCounts = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Sale
                         && h.Status != TaxDocumentStatus.Draft)
                .GroupBy(h => h.PartyUserID)
                .Select(g => new { UserID = g.Key, N = g.Count() })
                .ToListAsync();
            var docsByUser = docCounts.ToDictionary(x => x.UserID, x => x.N);

            var stateIdMap = await _resolver.StateIdMap();
            var report = new PartyStateReport();

            foreach (var u in users)
            {
                profileByUser.TryGetValue(u.UserMasterID, out var profile);

                string gstin = !string.IsNullOrWhiteSpace(profile?.GSTIN)
                    ? profile!.GSTIN.Trim()
                    : (u.GSTNo ?? string.Empty).Trim();

                // Work the sources out here rather than calling the resolver, because the screen
                // has to show WHICH source won, not just the answer.
                string profileCode = Clean(profile?.PlaceOfSupplyStateCode);
                string userCode = Clean(u.BusinessGSTStateCode);
                string gstinCode = gstin.Length == 15 ? Clean(gstin[..2]) : string.Empty;
                string onboardCode = (u.StateID ?? 0) > 0 && stateIdMap.TryGetValue(u.StateID!.Value, out var c)
                    ? c : string.Empty;

                (string code, string source) =
                      profileCode.Length > 0 ? (profileCode, PartyStateSource.Profile)
                    : userCode.Length > 0 ? (userCode, PartyStateSource.UserField)
                    : gstinCode.Length > 0 ? (gstinCode, PartyStateSource.Gstin)
                    : onboardCode.Length > 0 ? (onboardCode, PartyStateSource.OnboardingState)
                    : (string.Empty, PartyStateSource.None);

                var row = new PartyStateRow
                {
                    UserID = u.UserMasterID,
                    UserTypeID = u.UserTypeID ?? 0,
                    PartyName = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : (u.PersonName ?? string.Empty),
                    MobileNumber = u.MobileNumber ?? string.Empty,
                    GSTIN = gstin,
                    RegistrationStatus = !string.IsNullOrWhiteSpace(profile?.RegistrationStatus)
                        ? profile!.RegistrationStatus
                        : gstin.Length == 15 ? PartyRegistrationStatus.Registered : PartyRegistrationStatus.Unregistered,
                    ProfileStateCode = profileCode,
                    ResolvedStateCode = code,
                    ResolvedStateName = code.Length > 0 ? GstStates.Name(code) : string.Empty,
                    Source = source,
                    CanBackfill = code.Length > 0 && profileCode.Length == 0,
                    NeedsAttention = code.Length == 0,
                    IssuedDocuments = docsByUser.TryGetValue(u.UserMasterID, out var n) ? n : 0,
                };
                report.Parties.Add(row);
            }

            report.Parties = report.Parties
                .OrderByDescending(p => p.NeedsAttention)
                .ThenByDescending(p => p.CanBackfill)
                .ThenBy(p => p.PartyName)
                .ToList();

            report.Total = report.Parties.Count;
            report.AlreadySet = report.Parties.Count(p => p.ProfileStateCode.Length > 0);
            report.CanBackfill = report.Parties.Count(p => p.CanBackfill);
            report.NeedsAttention = report.Parties.Count(p => p.NeedsAttention);

            // Documents the portal would reject. "00" is the value the old padding produced.
            report.DocumentsWithBadPlaceOfSupply = await _context.TaxInvoiceHeaders.AsNoTracking()
                .CountAsync(h => !h.IsDeleted && h.Direction == TaxDirection.Sale
                              && h.Status != TaxDocumentStatus.Draft
                              && h.DocumentType != TaxDocumentType.SelfInvoice
                              && (h.PlaceOfSupplyStateCode == string.Empty || h.PlaceOfSupplyStateCode == "00"));

            return report;
        }

        public async Task<(PartyStateBackfillResult? Result, string Failure)> Backfill(PartyStateBackfillRequest request)
        {
            var review = await Review();
            var targets = review.Parties
                .Where(p => p.CanBackfill && (request.OnlyUserID <= 0 || p.UserID == request.OnlyUserID))
                .ToList();

            var result = new PartyStateBackfillResult
            {
                DryRun = request.DryRun,
                StillUnknown = review.NeedsAttention,
            };

            if (targets.Count == 0)
            {
                result.Messages.Add(review.CanBackfill == 0
                    ? "Nothing to derive — every party whose state is known already has it on its tax profile."
                    : "No party matched the filter.");
                return (result, string.Empty);
            }

            foreach (var t in targets)
            {
                if (request.DryRun)
                {
                    result.ProfilesCreated++;
                    result.Messages.Add($"{t.PartyName}: would set {t.ResolvedStateCode} — {t.ResolvedStateName} (from {Describe(t.Source)}).");
                    continue;
                }

                var profile = await _context.PartyTaxProfiles
                    .FirstOrDefaultAsync(p => p.UserID == t.UserID && !p.IsDeleted);

                if (profile == null)
                {
                    profile = new PartyTaxProfile
                    {
                        UserID = t.UserID,
                        GSTIN = t.GSTIN,
                        RegistrationStatus = t.RegistrationStatus,
                        PlaceOfSupplyStateCode = t.ResolvedStateCode,
                        Remark = $"Place of supply derived from {Describe(t.Source)}.",
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                        IsDeleted = false,
                    };
                    _context.PartyTaxProfiles.Add(profile);
                    result.ProfilesCreated++;
                }
                else
                {
                    // Never overwrite a value somebody set by hand — CanBackfill already means the
                    // profile has none, but a concurrent edit is exactly the case worth guarding.
                    if (Clean(profile.PlaceOfSupplyStateCode).Length > 0) continue;
                    profile.PlaceOfSupplyStateCode = t.ResolvedStateCode;
                    profile.UpdatedateTime = DateTime.UtcNow;
                    if (string.IsNullOrWhiteSpace(profile.Remark))
                        profile.Remark = $"Place of supply derived from {Describe(t.Source)}.";
                    result.ProfilesUpdated++;
                }
            }

            if (!request.DryRun) await _context.SaveChangesAsync();

            result.Messages.Add(request.DryRun
                ? $"{targets.Count} party profile(s) would be written. Nothing was changed."
                : $"{result.ProfilesCreated} created, {result.ProfilesUpdated} updated.");

            if (review.NeedsAttention > 0)
            {
                result.Messages.Add(
                    $"{review.NeedsAttention} part{(review.NeedsAttention == 1 ? "y" : "ies")} still have no state " +
                    "anywhere on record and must be set by hand — nothing can derive one for them.");
            }

            return (result, string.Empty);
        }

        public async Task<string> SetPlaceOfSupply(SetPartyStateRequest request)
        {
            if (request.UserID <= 0) return "A party is required.";

            string raw = (request.PlaceOfSupplyStateCode ?? string.Empty).Trim();

            // Typed input is validated STRICTLY, not cleaned. Clean() truncates to two characters,
            // which is right when reading a stored value or a GSTIN prefix but wrong here: it
            // would turn a typed "99x" into "99" and save a state nobody chose. One or two digits,
            // or nothing.
            if (raw.Length > 0 && (raw.Length > 2 || !raw.All(char.IsDigit)))
                return $"'{request.PlaceOfSupplyStateCode}' is not a GST state code — it is two digits, e.g. 27.";

            string code = Clean(raw);
            // An empty code CLEARS the field. That is deliberate: "we do not know" is a truthful
            // state, and recording "00" to look tidy is what caused the problem being fixed here.
            if (code.Length > 0 && !GstStates.IsKnown(code))
                return $"'{request.PlaceOfSupplyStateCode}' is not a GST state code.";

            bool exists = await _context.UserMasters.AnyAsync(u => u.UserMasterID == request.UserID && !u.IsDeleted);
            if (!exists) return "That party does not exist.";

            var profile = await _context.PartyTaxProfiles
                .FirstOrDefaultAsync(p => p.UserID == request.UserID && !p.IsDeleted);

            if (profile == null)
            {
                profile = new PartyTaxProfile
                {
                    UserID = request.UserID,
                    PlaceOfSupplyStateCode = code,
                    RegistrationStatus = PartyRegistrationStatus.Unregistered,
                    Remark = "Place of supply set by hand.",
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                    IsDeleted = false,
                };
                _context.PartyTaxProfiles.Add(profile);
            }
            else
            {
                profile.PlaceOfSupplyStateCode = code;
                profile.UpdatedateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<(DocumentRegenerationResult? Result, string Failure)> CancelForRegeneration(
            DocumentRegenerationRequest request)
        {
            string gstin = (request.CompanyGSTIN ?? string.Empty).Trim().ToUpperInvariant();
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6) return (null, "A period (yyyyMM) is required.");
            if (gstin.Length != 15) return (null, "The company GSTIN is required.");

            // A filed return is a declaration already made. Withdrawing a document it reported is
            // not a correction, it is a restatement — and the lawful route for that is a credit
            // note and a fresh invoice, which the Credit-Note screen does.
            bool filed = await _context.GstReturnFilings.AsNoTracking()
                .AnyAsync(f => !f.IsDeleted && f.CompanyGSTIN == gstin && f.PeriodKey == period
                            && f.ReturnType == GstReturnType.Gstr1 && f.Status == GstFilingStatus.Filed);
            if (filed)
            {
                return (null,
                    $"GSTR-1 for {Gstr1Service.PeriodLabel(period)} has already been filed, so its documents " +
                    "cannot be withdrawn. Issue a credit note against each one and raise a fresh invoice.");
            }

            // The ledger's own seal on a closed month.
            bool locked = await _context.AcctPeriodLocks.AsNoTracking()
                .AnyAsync(l => !l.IsDeleted && l.PeriodKey == period);
            if (locked)
            {
                return (null,
                    $"{Gstr1Service.PeriodLabel(period)} is sealed in the ledger. Unlock the period first if " +
                    "this really has to be redone.");
            }

            var bad = await _context.TaxInvoiceHeaders
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Sale
                         && h.CompanyGSTIN == gstin && h.PeriodKey == period
                         && h.Status == TaxDocumentStatus.Issued
                         && h.DocumentType != TaxDocumentType.SelfInvoice
                         && (h.PlaceOfSupplyStateCode == string.Empty || h.PlaceOfSupplyStateCode == "00"))
                .ToListAsync();

            var result = new DocumentRegenerationResult
            {
                DryRun = request.DryRun,
                PeriodKey = period,
                DocumentsCancelled = bad.Count,
                TaxableValueCancelled = Math.Round(bad.Sum(h => h.TaxableTotal), 2),
                DocumentNumbers = bad.Select(h => h.DocumentNo).OrderBy(n => n, StringComparer.Ordinal)
                                     .Take(50).ToList(),
            };

            if (bad.Count == 0)
            {
                result.Messages.Add($"No document in {Gstr1Service.PeriodLabel(period)} has an invalid place of supply.");
                return (result, string.Empty);
            }

            if (!request.DryRun)
            {
                // Cancels the headers AND releases their sources. Without the second half the
                // NextStep below — "re-run Generate for the same period" — found every source
                // already invoiced and produced nothing at all.
                await DocumentCancellation.Cancel(_context, bad,
                    "Cancelled: invalid place of supply. Regenerate after fixing the party.");
                await _context.SaveChangesAsync();
                result.Messages.Add($"{bad.Count} document(s) cancelled. Their numbers are kept and will be " +
                                    "reported as cancelled in GSTR-1 table 13.");
            }
            else
            {
                result.Messages.Add($"{bad.Count} document(s) would be cancelled. Nothing was changed.");
            }

            result.NextStep =
                "Now re-run Generate on the GST Documents screen for the same period. The parties' corrected " +
                "place of supply is read at generation, so the new documents carry the right tax heads.";
            return (result, string.Empty);
        }

        private static string Describe(string source) => source switch
        {
            PartyStateSource.UserField => "the party's own GST state field",
            PartyStateSource.Gstin => "the first two digits of the GSTIN",
            PartyStateSource.OnboardingState => "the state picked at onboarding",
            PartyStateSource.Profile => "the tax profile",
            _ => "nothing",
        };

        /// <summary>Two digits or empty. "00" is read as empty — it is what the old padding wrote
        /// for an unknown state, and passing it on is the bug this whole service exists to end.</summary>
        private static string Clean(string? raw)
        {
            string s = (raw ?? string.Empty).Trim();
            if (s.Length == 0) return string.Empty;
            if (s.Length > 2) s = s[..2];
            s = s.PadLeft(2, '0');
            return s == "00" ? string.Empty : s;
        }
    }
}
