using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase4;
using FintechApp.Requests.TaxEngine;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ISettlementAccountingService" />
    public class SettlementAccountingService : ISettlementAccountingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxEngineService _engine;
        private readonly ITaxConfigurationRepository _config;
        private readonly ILedgerPostingService _ledger;
        private readonly IInvoiceSettingRepository _invoiceSettings;
        private readonly ICommissionTaxResolver _commissionTax;
        private readonly IChargeGstResolver _chargeGst;
        private readonly ITdsDeductionService _tds;
        private readonly Logger _logger;

        public SettlementAccountingService(
            ApplicationDbContext context,
            ITaxEngineService engine,
            ITaxConfigurationRepository config,
            ILedgerPostingService ledger,
            IInvoiceSettingRepository invoiceSettings,
            ICommissionTaxResolver commissionTax,
            IChargeGstResolver chargeGst,
            ITdsDeductionService tds)
        {
            _context = context;
            _engine = engine;
            _config = config;
            _ledger = ledger;
            _invoiceSettings = invoiceSettings;
            _commissionTax = commissionTax;
            _chargeGst = chargeGst;
            _tds = tds;
            _logger = new Logger();
        }

        // ── Transaction settlement ─────────────────────────────────────────────────────────

        public async Task OnTransactionSettled(int transactionMasterID)
        {
            try
            {
                var txn = await _context.TransactionMasters.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TransactionMasterID == transactionMasterID);
                if (txn == null) return;

                // Split every charge on this transaction into what it is worth and what of it is
                // tax, and stamp it. This runs FIRST because the rest of the settlement reads it:
                // the sale stamp takes its inclusive/exclusive reading from the same decision, and
                // TDS deducts on the base it produced. Only a SUCCESS is ever stamped — a failed
                // transaction is refunded in full and owes nothing.
                var charges = await StampChargeGst(txn);

                var tax = await StampTax(txn, charges);
                if (tax == null) return;

                await EnqueueSaleEvents(txn, tax);

                // TDS on the commissions, when the profile says it is on. It is off everywhere by
                // default, so this is a no-op until somebody turns it on deliberately — which is
                // right, because turning it on withholds real money from real people.
                await _tds.DeductForTransaction(txn.TransactionMasterID);
            }
            catch (Exception ex)
            {
                // The wallet has already moved. Record and carry on — see the interface note.
                _logger.Write(ex);
            }
        }

        public async Task OnTransactionRefunded(int transactionMasterID)
        {
            try
            {
                var txn = await _context.TransactionMasters
                    .FirstOrDefaultAsync(t => t.TransactionMasterID == transactionMasterID);
                if (txn == null || txn.ChargeGSTStampedOn == null) return;

                // Back to zero, in full. A failed transaction earns nothing and is charged nothing:
                // the provider does not bill GST on a transfer that never happened, and the
                // retailer's commission went back with the principal.
                txn.ProviderChargeType = string.Empty;
                txn.ProviderGSTType = string.Empty;
                txn.ProviderGSTPercent = 0m;
                txn.ProviderGSTAmount = 0m;
                txn.ProviderBaseCharge = 0m;
                txn.ProviderTotalCharge = 0m;
                txn.ProviderGSTSource = string.Empty;

                txn.RetailerChargeType = string.Empty;
                txn.RetailerGSTType = string.Empty;
                txn.RetailerGSTPercent = 0m;
                txn.RetailerGST = 0m;
                txn.RetailerBaseCharge = 0m;
                txn.RetailerTotalCharge = 0m;
                txn.RetailerGSTSource = string.Empty;

                txn.DistributerGST = 0m;
                txn.DistributerBaseCharge = 0m;
                txn.SuperDistributerGST = 0m;
                txn.SuperDistributerBaseCharge = 0m;

                txn.ChargeGSTStampedOn = null;
                txn.UpdatedateTime = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // The refund itself has already happened. Same contract as everything else here.
                _logger.Write(ex);
            }
        }

        public async Task<bool> StampChargeGstForSettledTransaction(int transactionMasterID)
        {
            var txn = await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionMasterID == transactionMasterID);
            if (txn == null || txn.ChargeGSTStampedOn != null) return false;
            if (!txn.Status.EqualsIgnoreCase(StatusType.Success)) return false;

            await StampChargeGst(txn);

            return await _context.TransactionMasters.AsNoTracking()
                .AnyAsync(t => t.TransactionMasterID == transactionMasterID && t.ChargeGSTStampedOn != null);
        }

        /// <summary>
        /// Split and stamp every charge on one settled transaction: what the provider charged us,
        /// and what each downline tier earned, each into base + GST under the configuration that
        /// applies to it.
        ///
        /// Stamped once and never moved. A status-check sweep, a reconciliation run and a manual
        /// settle can all land on the same transaction, and a report that reads the stamp has to
        /// keep getting the same answer even after the rate card is edited — the figures belong to
        /// the card the transaction was priced on, not to today's.
        ///
        /// Returns the breakdown either way, so the caller can read the decision without having to
        /// know whether this call did the writing.
        /// </summary>
        private async Task<StampedCharges?> StampChargeGst(TransactionMaster txn)
        {
            try
            {
                // Only a success is stamped. OnTransactionSettled is only called on success, and
                // this is the belt to that braces: a refund clears the stamp, and the two must
                // never be able to fight over the same row.
                if (txn.Status.EqualsIgnoreCase(StatusType.Failed)) return null;

                if (txn.ChargeGSTStampedOn != null)
                {
                    // Already stamped. Read the decision back off the row rather than resolving it
                    // again, which is the whole point of storing it.
                    return StampedCharges.FromTransaction(txn);
                }

                var provider = await _chargeGst.ForProviderCharge(
                    txn.ProviderID, txn.ServiceID, txn.OperatorID, txn.Amount, txn.ProviderCommission, txn.DateTime);

                var downline = await _chargeGst.ForSlabSide(
                    txn.UserID, txn.ServiceID, txn.OperatorID, txn.Amount,
                    txn.RetailerCommission, txn.DistributerCommission, txn.SuperDistributerCommission, txn.DateTime);

                var tracked = await _context.TransactionMasters
                    .FirstOrDefaultAsync(t => t.TransactionMasterID == txn.TransactionMasterID);
                if (tracked == null) return null;

                // Another settlement path got there first. Its stamp stands.
                if (tracked.ChargeGSTStampedOn != null) return StampedCharges.FromTransaction(tracked);

                tracked.ProviderChargeType = provider.ChargeType;
                tracked.ProviderGSTType = provider.GSTType;
                tracked.ProviderGSTPercent = provider.GSTRatePercent;
                tracked.ProviderGSTAmount = provider.GSTAmount;
                tracked.ProviderBaseCharge = provider.BaseCharge;
                tracked.ProviderTotalCharge = provider.TotalCharge;
                tracked.ProviderGSTSource = Trim(provider.DecidedBy, 200);

                tracked.RetailerChargeType = downline.Retailer.ChargeType;
                tracked.RetailerGSTType = downline.Retailer.GSTType;
                tracked.RetailerGSTPercent = downline.Retailer.GSTRatePercent;
                tracked.RetailerGST = downline.Retailer.GSTAmount;
                tracked.RetailerBaseCharge = downline.Retailer.BaseCharge;
                tracked.RetailerTotalCharge = downline.Retailer.TotalCharge;
                tracked.RetailerGSTSource = Trim(downline.Retailer.DecidedBy, 200);

                tracked.DistributerGST = downline.Distributor.GSTAmount;
                tracked.DistributerBaseCharge = downline.Distributor.BaseCharge;
                tracked.SuperDistributerGST = downline.SuperDistributor.GSTAmount;
                tracked.SuperDistributerBaseCharge = downline.SuperDistributor.BaseCharge;

                tracked.ChargeGSTStampedOn = DateTime.Now;
                tracked.UpdatedateTime = DateTime.Now;

                await _context.SaveChangesAsync();

                return new StampedCharges(provider.GSTType, downline.Retailer.GSTType,
                    provider.BaseCharge, downline.Retailer.BaseCharge);
            }
            catch (Exception ex)
            {
                // A tax split must never undo a settlement. Unstamped is visible — and recoverable
                // by re-settling — where a thrown exception would leave money mid-flight.
                _logger.Write(ex);
                return null;
            }
        }

        /// <summary>The part of the stamp the rest of the settlement needs to read.</summary>
        private sealed record StampedCharges(
            string ProviderGSTType, string RetailerGSTType, decimal ProviderBase, decimal RetailerBase)
        {
            public static StampedCharges FromTransaction(TransactionMaster txn) => new(
                txn.ProviderGSTType ?? string.Empty, txn.RetailerGSTType ?? string.Empty,
                txn.ProviderBaseCharge, txn.RetailerBaseCharge);
        }

        /// <summary>
        /// Resolve and stamp the tax for one settled transaction. Returns the stamped row, or the
        /// existing one when this transaction was already stamped — a status-check sweep and a
        /// manual settle can both land on the same transaction, and the stamp must not move once
        /// written, because reports read it instead of recomputing.
        /// </summary>
        private async Task<TransactionTaxDetail?> StampTax(TransactionMaster txn, StampedCharges? charges)
        {
            var existing = await _context.TransactionTaxDetails.AsNoTracking()
                .FirstOrDefaultAsync(d => d.TransactionMasterID == txn.TransactionMasterID && !d.IsDeleted);
            if (existing != null) return existing;

            var party = await LoadParty(txn.UserID);
            string supplierState = await SupplierStateCode();

            // What WE charged this party. ProviderCommission is money the provider pays US — a
            // different document to a different counterparty — so it is deliberately not mixed in,
            // exactly as TaxInvoiceService leaves it out.
            decimal commission = txn.RetailerCommission > 0 ? txn.RetailerCommission : 0m;
            decimal charge = txn.DebitAmount > txn.Amount ? txn.DebitAmount - txn.Amount : 0m;

            // Whether the figure already contains the tax is CONFIGURED, not assumed. It used to
            // be the literal `true` here, which quietly made a provider paying commission
            // exclusive of GST look identical to one paying it inclusive. The slab the user is on
            // lets a SLAB-scoped profile win over the service or provider default.
            int slabMasterId = await _context.UserSlabMappings.AsNoTracking()
                .Where(m => m.UserID == txn.UserID && !m.IsDeleted)
                .Select(m => m.SlabMasterID).FirstOrDefaultAsync();

            var treatment = await _commissionTax.Resolve(
                txn.ServiceID, txn.OperatorID, txn.ProviderID, slabMasterId, txn.DateTime);

            // The band the retailer was actually priced on gets the last word, because that is the
            // most specific thing anyone configured: a slab band may be EXCLUSIVE while the
            // service profile is INCLUSIVE, and the sale we invoice has to agree with the charge we
            // took. Falling back to the profile keeps every transaction settled before this
            // shipped reading exactly as it did.
            string taxMode = !string.IsNullOrWhiteSpace(charges?.RetailerGSTType)
                ? charges!.RetailerGSTType
                : treatment.TaxMode;

            var result = await _engine.Resolve(new TaxResolutionContext
            {
                Direction = TaxDirection.Sale,
                ServiceID = txn.ServiceID,
                OperatorID = txn.OperatorID,
                ProviderID = txn.ProviderID,
                PartyUserID = txn.UserID,
                UserTypeID = party.UserTypeID,
                PartyRegistrationStatus = party.RegistrationStatus,
                TransactionDate = txn.DateTime,
                GrossValue = txn.Amount,
                CommissionValue = commission,
                ChargeValue = charge,
                IsTaxInclusive = taxMode != CommissionTaxMode.Exclusive,
                SupplierStateCode = supplierState,
                PlaceOfSupplyStateCode = party.StateCode,
            });

            var detail = new TransactionTaxDetail
            {
                TransactionMasterID = txn.TransactionMasterID,
                Direction = result.Direction,
                TaxRuleMasterID = result.TaxRuleMasterID,
                BillingBasis = result.BillingBasis,
                GrossValue = Round(result.GrossValue),
                TaxableValue = Round(result.TaxableValue),
                SupplyNature = result.SupplyNature,
                SupplierStateCode = result.SupplierStateCode,
                PlaceOfSupplyStateCode = result.PlaceOfSupplyStateCode,
                SACCode = result.SACCode,
                CGSTRate = result.CGSTRate,
                CGSTAmount = Round(result.CGSTAmount),
                SGSTRate = result.SGSTRate,
                SGSTAmount = Round(result.SGSTAmount),
                IGSTRate = result.IGSTRate,
                IGSTAmount = Round(result.IGSTAmount),
                CessAmount = Round(result.CessAmount),
                TDSSectionCode = result.TDSSectionCode,
                TDSRate = result.TDSRate,
                TDSAmount = Round(result.TDSAmount),
                PartyRelationship = result.PartyRelationship,
                ITCEligible = result.ITCEligible,
                ReverseCharge = result.ReverseCharge,
                ResolutionWarning = Trim(result.Warning, 300),
                ResolvedOn = result.ResolvedOn == default ? DateTime.UtcNow : result.ResolvedOn,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
                IsDeleted = false,
            };

            // Already stamped by another settlement path (a status check and a callback racing on
            // the same transaction, or a re-settle). Checked before the insert so the ordinary case
            // never has to throw its way to the same answer — the unique index still backs this up.
            var stamped = await _context.TransactionTaxDetails.AsNoTracking()
                .FirstOrDefaultAsync(d => d.TransactionMasterID == txn.TransactionMasterID && !d.IsDeleted);
            if (stamped != null) return stamped;

            _context.TransactionTaxDetails.Add(detail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two settlement paths raced on the same transaction. The other one won; its row is
                // the stamp, and re-reading it is the right answer rather than a second row.
                _context.Entry(detail).State = EntityState.Detached;
                return await _context.TransactionTaxDetails.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.TransactionMasterID == txn.TransactionMasterID && !d.IsDeleted);
            }

            return detail;
        }

        /// <summary>
        /// Turn a stamped sale into ledger events. The basis decides WHICH event, which is the whole
        /// reason this lives in one place:
        ///
        ///  - GROSS (trading: talktime bought and resold) — the entire throughput is our turnover,
        ///    so SERVICE_SALE_GROSS books taxable to Service Income and the tax to Output GST.
        ///  - COMMISSION / CHARGE (agency: DMT, AEPS, payout) — only what we earned is ours, so
        ///    COMMISSION_EARNED books the earning and TAX_OUTPUT the GST on it.
        ///
        ///    NOTE, and this is a real gap rather than an oversight: on an agency service the
        ///    PRINCIPAL the retailer was debited (the money that goes to the beneficiary) is not
        ///    posted here. Where it belongs — against provider stock, against bank, or against a
        ///    settlement account — differs per rail and is one of the open questions for the CA, and
        ///    a posting rule invented here would quietly put a crore on the wrong account. Until
        ///    that rule is seeded, the User Wallet control account will sit short of the real wallet
        ///    total by the principal of every agency sale, and the daily wallet-vs-control check is
        ///    what reports it. A visible, explained difference beats an invented entry.
        ///
        ///  - Anything else (EXEMPT, NONGST, or an unresolved basis) is still enqueued, under an
        ///    event name no rule matches, so the outbox records it as Skipped WITH ITS REASON. That
        ///    is deliberate: an unposted sale that shows on a screen is a configuration backlog
        ///    item, and one that was never enqueued is nothing at all.
        /// </summary>
        private async Task EnqueueSaleEvents(TransactionMaster txn, TransactionTaxDetail tax)
        {
            string basis = (tax.BillingBasis ?? string.Empty).Trim().ToUpperInvariant();
            decimal totalTax = tax.CGSTAmount + tax.SGSTAmount + tax.IGSTAmount + tax.CessAmount;
            string narration = $"{BasisLabel(basis)} sale, txn {txn.TransactionMasterID}";

            if (basis == BillingBasis.Gross)
            {
                await Enqueue(LedgerEvent.ServiceSaleGross, txn.TransactionMasterID, txn.DateTime, narration,
                    new Dictionary<string, decimal>
                    {
                        ["Taxable"] = tax.TaxableValue,
                        ["Tax"] = totalTax,
                    });
                return;
            }

            if (basis == BillingBasis.Commission || basis == BillingBasis.Charge)
            {
                await Enqueue(LedgerEvent.CommissionEarned, txn.TransactionMasterID, txn.DateTime, narration,
                    new Dictionary<string, decimal> { ["Commission"] = tax.TaxableValue });

                if (totalTax > 0m)
                {
                    await Enqueue(LedgerEvent.TaxOutput, txn.TransactionMasterID, txn.DateTime,
                        $"Output GST, txn {txn.TransactionMasterID}",
                        new Dictionary<string, decimal> { ["Tax"] = totalTax });
                }
                return;
            }

            // No basis we can post. Enqueued anyway so the outbox carries the reason.
            await Enqueue($"SERVICE_SALE_{(basis.Length == 0 ? "UNRESOLVED" : basis)}",
                txn.TransactionMasterID, txn.DateTime,
                $"Sale with basis '{(basis.Length == 0 ? "unresolved" : basis)}', txn {txn.TransactionMasterID}",
                new Dictionary<string, decimal>
                {
                    ["Taxable"] = tax.TaxableValue,
                    ["Tax"] = totalTax,
                    ["Amount"] = tax.GrossValue,
                });
        }

        private static string BasisLabel(string basis) => basis switch
        {
            BillingBasis.Gross => "Gross service",
            BillingBasis.Commission => "Commission",
            BillingBasis.Charge => "Charge",
            _ => "Service",
        };

        // ── The two non-transaction events ────────────────────────────────────────────────

        public async Task OnWalletToppedUp(int virtualBalanceID, int walletID, decimal amount, DateTime when)
        {
            try
            {
                if (amount <= 0m) return;
                await Enqueue(LedgerEvent.WalletTopUp, 0, when,
                    $"Float top-up into wallet {walletID} (VirtualBalance {virtualBalanceID})",
                    new Dictionary<string, decimal> { ["Amount"] = Round(amount) },
                    eventKey: $"{LedgerEvent.WalletTopUp}:VirtualBalance:{virtualBalanceID}");
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        public async Task OnProviderPurchase(int providerPurchaseID, int providerID, decimal amount, DateTime when)
        {
            try
            {
                if (amount <= 0m) return;
                await Enqueue(LedgerEvent.ProviderPurchase, 0, when,
                    $"Stock purchased from provider {providerID} (ProviderPurchase {providerPurchaseID})",
                    new Dictionary<string, decimal> { ["Amount"] = Round(amount) },
                    eventKey: $"{LedgerEvent.ProviderPurchase}:ProviderPurchase:{providerPurchaseID}");
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        // ── Plumbing ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// One enqueue. The key is what makes every caller safe to repeat: a status-check sweep, a
        /// manual settle and a reconciliation run can all settle the same transaction, and the
        /// second and third enqueue are no-ops on the same key.
        /// </summary>
        private Task Enqueue(string eventType, int txnId, DateTime when, string narration,
                             Dictionary<string, decimal> amounts, string? eventKey = null)
            => _ledger.Enqueue(new EnqueuePostingRequest
            {
                EventType = eventType,
                EventKey = eventKey ?? $"{eventType}:Txn:{txnId}",
                EventDate = when,
                Narration = narration,
                Amounts = amounts,
            });

        private sealed record PartyInfo(int UserTypeID, string RegistrationStatus, string StateCode);

        /// <summary>
        /// The party as the tax engine needs to see them. A missing profile is read as unregistered,
        /// which routes the turnover to B2C rather than asserting a registration nobody verified —
        /// the same reading TaxInvoiceService takes.
        /// </summary>
        private async Task<PartyInfo> LoadParty(int userId)
        {
            var user = await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserMasterID == userId)
                .Select(u => new { u.UserTypeID, u.GSTNo, u.BusinessGSTStateCode })
                .FirstOrDefaultAsync();
            if (user == null) return new PartyInfo(0, PartyRegistrationStatus.Unregistered, string.Empty);

            var profile = await _config.GetPartyTaxProfile(userId);

            string gstin = !string.IsNullOrWhiteSpace(profile?.GSTIN)
                ? profile!.GSTIN.Trim()
                : (user.GSTNo ?? string.Empty).Trim();

            string status = !string.IsNullOrWhiteSpace(profile?.RegistrationStatus)
                ? profile!.RegistrationStatus.Trim().ToUpperInvariant()
                : gstin.Length == 15 ? PartyRegistrationStatus.Registered : PartyRegistrationStatus.Unregistered;

            string state = !string.IsNullOrWhiteSpace(profile?.PlaceOfSupplyStateCode)
                ? profile!.PlaceOfSupplyStateCode.Trim()
                : !string.IsNullOrWhiteSpace(user.BusinessGSTStateCode)
                    ? user.BusinessGSTStateCode!.Trim()
                    // The first two digits of a GSTIN ARE its state code.
                    : gstin.Length == 15 ? gstin.Substring(0, 2) : string.Empty;

            // UserTypeID is nullable on the entity; 0 is the engine's "any tier" and is the right
            // reading for a user whose type was never set.
            return new PartyInfo(user.UserTypeID ?? 0, status, state);
        }

        /// <summary>Our own state, from the company GSTIN on Invoice Settings.</summary>
        private async Task<string> SupplierStateCode()
        {
            var settings = (await _invoiceSettings.GetAll()).FirstOrDefault();
            string gstin = (settings?.GSTIN ?? string.Empty).Trim();
            return gstin.Length >= 2 ? gstin.Substring(0, 2) : string.Empty;
        }

        private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s.Substring(0, max));
    }
}
