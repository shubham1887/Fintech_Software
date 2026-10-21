using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxEngine;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxEngine;
using FintechApp.Responses.TaxInvoice;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <summary>
    /// See <see cref="ITaxInvoiceService"/>.
    ///
    /// The engine decides the tax; this decides WHAT gets a document and WHO it is addressed to.
    /// It never computes a rate itself.
    /// </summary>
    public class TaxInvoiceService : ITaxInvoiceService
    {
        private const string SeriesTaxInvoice = "SALE-TAX";

        private readonly ApplicationDbContext _context;
        private readonly ITaxEngineService _engine;
        private readonly ITaxDocumentNumberService _numbers;
        private readonly ITaxConfigurationRepository _config;

        public TaxInvoiceService(ApplicationDbContext context, ITaxEngineService engine,
                                 ITaxDocumentNumberService numbers, ITaxConfigurationRepository config)
        {
            _context = context;
            _engine = engine;
            _numbers = numbers;
            _config = config;
        }

        // ── Wallet top-ups ──────────────────────────────────────────────────────────────

        public async Task<InvoiceGenerationResult> GenerateWalletTopUpInvoices(InvoiceGenerationRequest request)
        {
            var result = NewResult(request);
            DateTime periodStart = request.PeriodFrom.Date;  // not "from": that is a C# query keyword
            DateTime periodEnd = request.PeriodTo.Date.AddDays(1);

            var grossWallets = await ResolveGrossBasisWallets(request.PeriodTo, result.Warnings);
            if (grossWallets.Count == 0)
            {
                result.Warnings.Add("No wallet qualifies for gross top-up invoicing, so nothing was generated here. "
                                  + "Commission and charge revenue is invoiced by GenerateCommissionInvoices instead.");
                return result;
            }

            // Successful credits INTO a non-admin party. A credit to the admin is funding coming
            // in, not a supply going out.
            var topUps = await (from f in _context.FundTransactions.AsNoTracking()
                                join u in _context.UserMasters.AsNoTracking() on f.ReceiverID equals u.UserMasterID
                                where !f.IsDeleted
                                   && f.DateTime >= periodStart && f.DateTime < periodEnd
                                   && grossWallets.Contains(f.WalletID)
                                   && u.UserTypeID != UserTypeCode.Admin
                                   && (request.OnlyPartyUserID == 0 || f.ReceiverID == request.OnlyPartyUserID)
                                select new { f.FundTransactionID, f.ReceiverID, f.WalletID, f.Amount, f.DateTime, f.Status })
                               .ToListAsync();

            topUps = topUps.Where(t => string.Equals(t.Status, StatusType.Success, StringComparison.OrdinalIgnoreCase)).ToList();
            if (topUps.Count == 0) return result;

            // A credit to an exchange partner is ALSO a wallet top-up, so without this both this
            // generator and the exchange path would claim the same movement. The unique index
            // would stop the duplicate, but whichever ran first would own it — and if this one
            // won, the supply would be billed as an ordinary B2C sale while its exchange leg sat
            // awaiting a document that could never be raised, because its source was spent.
            // Exchange supplies belong to IExchangeLegService, which links the document to the leg.
            var exchangePartners = (await _context.PartyTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsExchangePartner)
                .Select(p => p.UserID).ToListAsync()).ToHashSet();

            if (exchangePartners.Count > 0)
            {
                int before = topUps.Count;
                topUps = topUps.Where(t => !exchangePartners.Contains(t.ReceiverID)).ToList();
                int handedOver = before - topUps.Count;
                if (handedOver > 0)
                    result.Warnings.Add($"{handedOver} credit(s) to exchange partner(s) were left to the exchange "
                                      + "register, which invoices them against their leg. They are supplies under "
                                      + "GST s.7(1)(a), not ordinary top-ups.");
            }
            if (topUps.Count == 0) return result;

            var alreadyDone = await InvoicedFundTransactionIds(topUps.Select(t => t.FundTransactionID).ToList());
            var pending = topUps.Where(t => !alreadyDone.Contains(t.FundTransactionID)).ToList();
            result.SkippedAlreadyInvoiced += topUps.Count - pending.Count;
            if (pending.Count == 0) return result;

            // A wallet serves several services; the invoice needs one to carry the SAC, so take
            // the lowest-numbered active service mapped to it. They all resolve to GROSS or the
            // wallet would not have qualified above, so the SAC is the only thing at stake.
            var walletService = await WalletRepresentativeService();

            foreach (var partyGroup in pending.GroupBy(t => t.ReceiverID))
            {
                var party = await LoadParty(partyGroup.Key);
                if (party == null)
                {
                    result.SkippedUnresolved += partyGroup.Count();
                    result.Warnings.Add($"User {partyGroup.Key} not found; {partyGroup.Count()} top-up(s) not invoiced.");
                    continue;
                }

                // Registered: one document per top-up, because a registered buyer needs an invoice
                // it can match line by line against its own purchase register.
                // Unregistered: one document per day, which is what GST allows for B2C and what
                // stops a busy counter producing forty documents a day.
                bool consolidate = party.RegistrationStatus != PartyRegistrationStatus.Registered;

                var batches = consolidate
                    ? partyGroup.GroupBy(t => t.DateTime.Date).Select(g => (Date: g.Key, Items: g.ToList())).ToList()
                    : partyGroup.Select(t => (Date: t.DateTime.Date, Items: new[] { t }.ToList())).ToList();

                foreach (var batch in batches)
                {
                    int serviceId = walletService.TryGetValue(batch.Items[0].WalletID, out int s) ? s : 0;
                    decimal gross = batch.Items.Sum(i => i.Amount);

                    var tax = await _engine.Resolve(new TaxResolutionContext
                    {
                        Direction = TaxDirection.Sale,
                        ServiceID = serviceId,
                        PartyUserID = party.UserID,
                        UserTypeID = party.UserTypeID,
                        PartyRegistrationStatus = party.RegistrationStatus,
                        TransactionDate = batch.Date,
                        GrossValue = gross,
                        // A wallet top-up is collected inclusive of tax: the retailer pays
                        // Rs 10,000 and gets Rs 10,000 of balance, exactly as Ezytm charged
                        // Rs 5,50,000 for Rs 5,50,000 of talktime.
                        IsTaxInclusive = true,
                        SupplierStateCode = request.SupplierStateCode,
                        PlaceOfSupplyStateCode = party.StateCode,
                    });

                    if (!string.Equals(tax.BillingBasis, BillingBasis.Gross, StringComparison.OrdinalIgnoreCase))
                    {
                        result.SkippedUnresolved += batch.Items.Count;
                        result.Warnings.Add($"Service {serviceId} on wallet {batch.Items[0].WalletID} resolved to "
                                          + $"'{tax.BillingBasis}', not GROSS, so {batch.Items.Count} top-up(s) for "
                                          + $"party {party.UserID} were not invoiced. {tax.Warning}");
                        continue;
                    }

                    await WriteDocument(request, result, party, tax, serviceId, batch.Date, consolidate,
                        description: consolidate ? $"Wallet top-ups, {batch.Date:dd-MMM-yyyy}" : "Wallet top-up",
                        sources: batch.Items.Select(i => (TxnId: 0, FundId: i.FundTransactionID, i.DateTime,
                                                          Reference: string.Empty, Gross: i.Amount)).ToList());
                }
            }

            return result;
        }

        // ── Commission and charge ───────────────────────────────────────────────────────

        public async Task<InvoiceGenerationResult> GenerateCommissionInvoices(InvoiceGenerationRequest request)
        {
            var result = NewResult(request);
            DateTime periodStart = request.PeriodFrom.Date;  // not "from": that is a C# query keyword
            DateTime periodEnd = request.PeriodTo.Date.AddDays(1);

            var txns = await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                         && t.DateTime >= periodStart && t.DateTime < periodEnd
                         && (request.OnlyPartyUserID == 0 || t.UserID == request.OnlyPartyUserID))
                .Select(t => new
                {
                    t.TransactionMasterID, t.UserID, t.ServiceID, t.OperatorID, t.ProviderID,
                    t.Amount, t.AdminCommission, t.AdminGST, t.ProviderCommission, t.Status, t.DateTime, t.ReferenceNo,
                })
                .ToListAsync();

            txns = txns.Where(t => string.Equals(t.Status, StatusType.Success, StringComparison.OrdinalIgnoreCase)).ToList();
            if (txns.Count == 0) return result;

            var alreadyDone = await InvoicedTransactionIds(txns.Select(t => t.TransactionMasterID).ToList());
            var pending = txns.Where(t => !alreadyDone.Contains(t.TransactionMasterID)).ToList();
            result.SkippedAlreadyInvoiced += txns.Count - pending.Count;
            if (pending.Count == 0) return result;

            // One document per party per service for the period. Per service, not just per party,
            // because the SAC and the rate belong to the service and a single line cannot carry two.
            foreach (var group in pending.GroupBy(t => new { t.UserID, t.ServiceID }))
            {
                var party = await LoadParty(group.Key.UserID);
                if (party == null)
                {
                    result.SkippedUnresolved += group.Count();
                    result.Warnings.Add($"User {group.Key.UserID} not found; {group.Count()} transaction(s) not invoiced.");
                    continue;
                }

                decimal gross = group.Sum(t => t.Amount);
                // What WE charge this party. ProviderCommission is money the provider pays US and
                // is a different document to a different counterparty — see the note in the
                // service's remark below; it is deliberately not mixed in here.
                decimal ourCharge = group.Sum(t => t.AdminCommission + t.AdminGST);

                var tax = await _engine.Resolve(new TaxResolutionContext
                {
                    Direction = TaxDirection.Sale,
                    ServiceID = group.Key.ServiceID,
                    PartyUserID = party.UserID,
                    UserTypeID = party.UserTypeID,
                    PartyRegistrationStatus = party.RegistrationStatus,
                    TransactionDate = request.PeriodTo.Date,
                    GrossValue = gross,
                    CommissionValue = ourCharge,
                    ChargeValue = ourCharge,
                    // A per-transaction charge is deducted from the wallet inclusive of tax, the
                    // same way a top-up is collected inclusive of it.
                    IsTaxInclusive = true,
                    SupplierStateCode = request.SupplierStateCode,
                    PlaceOfSupplyStateCode = party.StateCode,
                });

                // GROSS belongs to the top-up generator; invoicing it here as well would bill the
                // same throughput twice.
                if (string.Equals(tax.BillingBasis, BillingBasis.Gross, StringComparison.OrdinalIgnoreCase))
                {
                    result.SkippedAlreadyInvoiced += group.Count();
                    continue;
                }

                if (tax.TaxableValue <= 0m)
                {
                    result.SkippedUnresolved += group.Count();
                    result.Warnings.Add($"Service {group.Key.ServiceID} for party {party.UserID}: nothing to bill "
                                      + $"(basis {tax.BillingBasis}, taxable 0 from {group.Count()} transaction(s)). "
                                      + "Either no charge was levied, or AdminCommission is not where this service records it.");
                    continue;
                }

                await WriteDocument(request, result, party, tax, group.Key.ServiceID, request.PeriodTo.Date,
                    consolidated: true,
                    description: $"{tax.BillingBasis.ToLowerInvariant()} for {request.PeriodFrom:dd-MMM} to {request.PeriodTo:dd-MMM-yyyy}",
                    sources: group.Select(t => (TxnId: t.TransactionMasterID, FundId: 0, t.DateTime,
                                                Reference: t.ReferenceNo ?? string.Empty, Gross: t.Amount)).ToList());
            }

            return result;
        }

        // ── Shared ──────────────────────────────────────────────────────────────────────

        private static InvoiceGenerationResult NewResult(InvoiceGenerationRequest r) => new()
        {
            DryRun = r.DryRun,
            PeriodFrom = r.PeriodFrom.Date,
            PeriodTo = r.PeriodTo.Date,
        };

        /// <summary>
        /// Which wallets may be invoiced on top-up.
        ///
        /// A wallet qualifies only when EVERY active service mapped to it resolves to GROSS. A
        /// wallet carrying both a gross service and a charge service cannot be invoiced on its
        /// top-up, because the same throughput would then be billed twice — once as the top-up and
        /// again as the charge. That is a configuration problem, not something to average over, so
        /// the wallet is skipped and named.
        /// </summary>
        private async Task<HashSet<int>> ResolveGrossBasisWallets(DateTime asOf, List<string> warnings)
        {
            var qualifying = new HashSet<int>();

            var mappings = await (from m in _context.WalletServiceMappings.AsNoTracking()
                                  join s in _context.ServiceMasters.AsNoTracking() on m.ServiceID equals s.ServiceID
                                  where !m.IsDeleted && !s.IsDeleted && s.IsActive
                                  select new { m.WalletID, m.ServiceID, s.Name }).ToListAsync();

            foreach (var wallet in mappings.GroupBy(m => m.WalletID))
            {
                var nonGross = new List<string>();
                foreach (var m in wallet)
                {
                    var probe = await _engine.Resolve(new TaxResolutionContext
                    {
                        Direction = TaxDirection.Sale,
                        ServiceID = m.ServiceID,
                        TransactionDate = asOf,
                        GrossValue = 0m,
                    });
                    if (!string.Equals(probe.BillingBasis, BillingBasis.Gross, StringComparison.OrdinalIgnoreCase))
                        nonGross.Add($"{m.Name} ({probe.BillingBasis})");
                }

                if (nonGross.Count == 0) qualifying.Add(wallet.Key);
                else
                    warnings.Add($"Wallet {wallet.Key} is not invoiced on top-up: {nonGross.Count} of "
                               + $"{wallet.Count()} mapped services are not GROSS — {string.Join(", ", nonGross.Take(5))}"
                               + (nonGross.Count > 5 ? ", ..." : string.Empty)
                               + ". Billing a mixed wallet on its top-up would charge the same throughput twice. "
                               + "Separate the services onto their own wallets, or invoice this wallet's revenue as commission.");
            }

            return qualifying;
        }

        private async Task<Dictionary<int, int>> WalletRepresentativeService()
        {
            var rows = await (from m in _context.WalletServiceMappings.AsNoTracking()
                              join s in _context.ServiceMasters.AsNoTracking() on m.ServiceID equals s.ServiceID
                              where !m.IsDeleted && !s.IsDeleted && s.IsActive
                              select new { m.WalletID, m.ServiceID }).ToListAsync();
            return rows.GroupBy(r => r.WalletID).ToDictionary(g => g.Key, g => g.Min(x => x.ServiceID));
        }

        private async Task<HashSet<int>> InvoicedFundTransactionIds(List<int> ids) =>
            (await _context.TaxInvoiceAnnexureLines.AsNoTracking()
                .Where(a => !a.IsDeleted && a.FundTransactionID > 0 && ids.Contains(a.FundTransactionID))
                .Select(a => a.FundTransactionID).ToListAsync()).ToHashSet();

        private async Task<HashSet<int>> InvoicedTransactionIds(List<int> ids) =>
            (await _context.TaxInvoiceAnnexureLines.AsNoTracking()
                .Where(a => !a.IsDeleted && a.TransactionMasterID > 0 && ids.Contains(a.TransactionMasterID))
                .Select(a => a.TransactionMasterID).ToListAsync()).ToHashSet();

        private sealed class PartyInfo
        {
            public int UserID { get; init; }
            public int UserTypeID { get; init; }
            public string Name { get; init; } = string.Empty;
            public string GSTIN { get; init; } = string.Empty;
            public string RegistrationStatus { get; init; } = string.Empty;
            public string StateCode { get; init; } = string.Empty;
        }

        /// <summary>
        /// The party's billing identity. The tax profile wins where one exists, because that is
        /// the verified record; UserMaster's own fields are what onboarding typed in and nothing
        /// there says whether anybody checked them.
        /// </summary>
        private async Task<PartyInfo?> LoadParty(int userId)
        {
            var user = await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserMasterID == userId)
                .Select(u => new { u.UserMasterID, u.UserTypeID, u.FirmName, u.PersonName, u.GSTNo, u.BusinessGSTStateCode })
                .FirstOrDefaultAsync();
            if (user == null) return null;

            var profile = await _config.GetPartyTaxProfile(userId);

            string gstin = !string.IsNullOrWhiteSpace(profile?.GSTIN) ? profile!.GSTIN.Trim()
                         : (user.GSTNo ?? string.Empty).Trim();

            string status = !string.IsNullOrWhiteSpace(profile?.RegistrationStatus)
                ? profile!.RegistrationStatus.Trim().ToUpperInvariant()
                // No profile yet, so fall back to whether a GSTIN is even present. Unregistered is
                // the safe reading: it routes the turnover to B2C rather than claiming a party is
                // registered on the strength of a string nobody verified.
                : (gstin.Length == 15 ? PartyRegistrationStatus.Registered : PartyRegistrationStatus.Unregistered);

            string state = !string.IsNullOrWhiteSpace(profile?.PlaceOfSupplyStateCode)
                ? profile!.PlaceOfSupplyStateCode.Trim()
                : !string.IsNullOrWhiteSpace(user.BusinessGSTStateCode)
                    ? user.BusinessGSTStateCode.Trim()
                    // The first two digits of a GSTIN ARE its state code, so derive it rather than
                    // leaving the tax head to a default.
                    : gstin.Length == 15 ? gstin.Substring(0, 2) : string.Empty;

            return new PartyInfo
            {
                UserID = user.UserMasterID,
                UserTypeID = user.UserTypeID ?? 0,
                Name = !string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName : (user.PersonName ?? string.Empty),
                GSTIN = gstin,
                RegistrationStatus = status,
                StateCode = state.PadLeft(2, '0'),
            };
        }

        // ── Exchange legs ───────────────────────────────────────────────────────────────

        public async Task<(GeneratedDocument? Document, string FailureMessage)> IssueExchangeLegInvoice(
            int exchangePartnerLegID, InvoiceGenerationRequest request)
        {
            var leg = await _context.ExchangePartnerLegs
                .FirstOrDefaultAsync(l => l.ExchangePartnerLegID == exchangePartnerLegID && !l.IsDeleted);
            if (leg == null) return (null, $"Exchange leg {exchangePartnerLegID} not found.");

            if (!string.Equals(leg.Direction, ExchangeLegDirection.Outward, StringComparison.OrdinalIgnoreCase))
                return (null, $"Leg {exchangePartnerLegID} is {leg.Direction}. An inward leg is the PARTNER's supply; "
                            + "their invoice has to be obtained and entered, not generated by us.");

            if (leg.TaxInvoiceHeaderID.HasValue)
                return (null, $"Leg {exchangePartnerLegID} already carries document {leg.TaxInvoiceHeaderID}.");

            var party = await LoadParty(leg.PartnerUserID);
            if (party == null) return (null, $"Partner {leg.PartnerUserID} not found.");

            var walletService = await WalletRepresentativeService();
            int serviceId = walletService.TryGetValue(leg.WalletID, out int s) ? s : 0;

            var tax = await _engine.Resolve(new TaxResolutionContext
            {
                Direction = TaxDirection.Sale,
                ServiceID = serviceId,
                PartyUserID = party.UserID,
                UserTypeID = party.UserTypeID,
                PartyRegistrationStatus = party.RegistrationStatus,
                TransactionDate = leg.LegDate.Date,
                GrossValue = leg.Amount,
                // Open market value of the supply, which for a wallet credit is the credited
                // amount, inclusive of tax exactly as an ordinary top-up is.
                IsTaxInclusive = true,
                SupplierStateCode = request.SupplierStateCode,
                PlaceOfSupplyStateCode = party.StateCode,
            });

            var result = NewResult(request);
            var doc = await WriteDocument(request, result, party, tax, serviceId, leg.LegDate.Date,
                consolidated: false,
                description: $"Exchange supply to {party.Name}, {leg.LegDate:dd-MMM-yyyy}",
                sources: new List<(int, int, DateTime, string, decimal)>
                {
                    (0, leg.FundTransactionID, leg.LegDate, $"EXCH-{leg.ExchangePartnerLegID}", leg.Amount),
                });

            if (doc == null || doc.TaxInvoiceHeaderID == 0)
                return (null, result.Warnings.Count > 0 ? string.Join(" | ", result.Warnings) : "Document was not written.");

            if (!request.DryRun)
            {
                // Link both ways. The header knows it is an exchange supply so a barter can never
                // be read back as an ordinary sale and lose its other half; the leg knows it is
                // settled so the awaiting-document queue clears.
                var header = await _context.TaxInvoiceHeaders.FirstAsync(h => h.TaxInvoiceHeaderID == doc.TaxInvoiceHeaderID);
                header.ExchangePartnerLegID = leg.ExchangePartnerLegID;
                header.UpdatedateTime = DateTime.UtcNow;

                leg.TaxInvoiceHeaderID = doc.TaxInvoiceHeaderID;
                leg.DocumentStatus = ExchangeDocumentStatus.Invoiced;
                leg.UpdatedateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return (doc, string.Empty);
        }

        /// <summary>Build and (unless this is a dry run) save one document with its line and annexure.</summary>
        private async Task<GeneratedDocument?> WriteDocument(
            InvoiceGenerationRequest request, InvoiceGenerationResult result, PartyInfo party,
            TaxResolutionResult tax, int serviceId, DateTime documentDate, bool consolidated,
            string description,
            List<(int TxnId, int FundId, DateTime DateTime, string Reference, decimal Gross)> sources)
        {
            var doc = new GeneratedDocument
            {
                DocumentType = "TaxInvoice",
                DocumentDate = documentDate,
                PartyUserID = party.UserID,
                PartyName = party.Name,
                PartyRegistrationStatus = party.RegistrationStatus,
                IsConsolidated = consolidated,
                SourceCount = sources.Count,
                GrossTotal = tax.GrossValue,
                TaxableTotal = tax.TaxableValue,
                TaxTotal = tax.TotalTax,
                GrandTotal = tax.InvoiceValue,
                BillingBasis = tax.BillingBasis,
            };

            if (!string.IsNullOrWhiteSpace(tax.Warning)) result.Warnings.Add($"Party {party.UserID}: {tax.Warning}");

            if (request.DryRun)
            {
                doc.DocumentNo = "(dry run)";
            }
            else
            {
                var (documentNo, failure) = await _numbers.Allocate(SeriesTaxInvoice, request.CompanyGSTIN, documentDate);
                if (failure.Length > 0)
                {
                    result.SkippedUnresolved += sources.Count;
                    result.Warnings.Add($"Party {party.UserID}: {failure}");
                    return null;
                }
                doc.DocumentNo = documentNo;

                var header = new TaxInvoiceHeader
                {
                    Direction = TaxDirection.Sale,
                    DocumentType = "TaxInvoice",
                    TaxDocumentSeriesID = null,
                    DocumentNo = documentNo,
                    DocumentDate = documentDate,
                    PartyUserID = party.UserID,
                    PartyName = party.Name,
                    PartyGSTIN = party.GSTIN,
                    PartyRegistrationStatus = party.RegistrationStatus,
                    CompanyGSTIN = request.CompanyGSTIN,
                    SupplierStateCode = request.SupplierStateCode,
                    PlaceOfSupplyStateCode = tax.PlaceOfSupplyStateCode,
                    SupplyNature = tax.SupplyNature,
                    GrossTotal = tax.GrossValue,
                    TaxableTotal = tax.TaxableValue,
                    CGSTTotal = tax.CGSTAmount,
                    SGSTTotal = tax.SGSTAmount,
                    IGSTTotal = tax.IGSTAmount,
                    CessTotal = tax.CessAmount,
                    RoundOff = tax.RoundOff,
                    GrandTotal = tax.InvoiceValue,
                    Status = "Issued",
                    PeriodKey = documentDate.ToString("yyyyMM"),
                    IsConsolidated = consolidated,
                    Remark = sources.Count > 1 ? $"Consolidated from {sources.Count} source(s)" : string.Empty,
                    CreatedByUserID = request.CreatedByUserID,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                };
                _context.TaxInvoiceHeaders.Add(header);
                await _context.SaveChangesAsync();

                var line = new TaxInvoiceLine
                {
                    TaxInvoiceHeaderID = header.TaxInvoiceHeaderID,
                    LineNo = 1,
                    ServiceID = serviceId,
                    Description = description,
                    SACCode = tax.SACCode,
                    Quantity = sources.Count,
                    BillingBasis = tax.BillingBasis,
                    GrossValue = tax.GrossValue,
                    TaxableValue = tax.TaxableValue,
                    CGSTRate = tax.CGSTRate,
                    CGSTAmount = tax.CGSTAmount,
                    SGSTRate = tax.SGSTRate,
                    SGSTAmount = tax.SGSTAmount,
                    IGSTRate = tax.IGSTRate,
                    IGSTAmount = tax.IGSTAmount,
                    CessAmount = tax.CessAmount,
                    LineTotal = tax.InvoiceValue,
                    TaxRuleMasterID = tax.TaxRuleMasterID,
                    ITCEligible = false,
                    ReverseCharge = tax.ReverseCharge,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                };
                _context.TaxInvoiceLines.Add(line);
                await _context.SaveChangesAsync();

                foreach (var src in sources)
                {
                    _context.TaxInvoiceAnnexureLines.Add(new TaxInvoiceAnnexureLine
                    {
                        TaxInvoiceHeaderID = header.TaxInvoiceHeaderID,
                        TaxInvoiceLineID = line.TaxInvoiceLineID,
                        TransactionMasterID = src.TxnId,
                        FundTransactionID = src.FundId,
                        TransactionDate = src.DateTime,
                        ReferenceNo = src.Reference,
                        GrossValue = src.Gross,
                        // Apportioned by share of the gross, so the annexure adds back to the line
                        // rather than drifting from it.
                        TaxableValue = tax.GrossValue == 0 ? 0 : Math.Round(tax.TaxableValue * src.Gross / tax.GrossValue, 2),
                        TaxAmount = tax.GrossValue == 0 ? 0 : Math.Round(tax.TotalTax * src.Gross / tax.GrossValue, 2),
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                    });
                }
                await _context.SaveChangesAsync();

                doc.TaxInvoiceHeaderID = header.TaxInvoiceHeaderID;
                result.LinesGenerated++;
                result.AnnexureRowsGenerated += sources.Count;
            }

            result.DocumentsGenerated++;
            result.GrossTotal += tax.GrossValue;
            result.TaxableTotal += tax.TaxableValue;
            result.TaxTotal += tax.TotalTax;
            result.Documents.Add(doc);
            return doc;
        }
    }
}
