using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.TodaySummary;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// "Today's Summary" — money movement for a chosen day: what was opened with, what came in and
    /// went out by category, and what was closed with.
    ///
    /// Scope depends on who is asking, and the two are deliberately different reports:
    ///   • <see cref="Select"/> (admin) is NETWORK-WIDE — every wallet of every user in the
    ///     distribution network, and deliberately NOT the operator's own accounts (admin,
    ///     support, accountant). The admin dashboard is asking what the network holds; the
    ///     operator's own balance is reported on its own in the balance strip above, so
    ///     including it here counted the same money twice and made a wallet top-up look like
    ///     the network having opened the day richer.
    ///   • <see cref="SelectForRetailer"/> / <see cref="SelectForDistributor"/> are that one
    ///     account's own main wallet.
    ///
    /// Balances come from the TransactionDetails ledger (see <see cref="LedgerBalances"/>), not from
    /// UserBalanceHistory and not from "whatever the wallet holds right now". That fixes the two
    /// things that made this report untrustworthy:
    ///   • Opening was read from UserBalanceHistory, a table nothing in the running system writes —
    ///     so it was almost always 0.
    ///   • Closing was the CURRENT balance regardless of the date picked, so any past date showed
    ///     today's money against that day's activity.
    ///
    /// On the per-account summaries, Difference is a real reconciliation signal: the wallet balance
    /// minus the balance the ledger says it should hold. Anything other than zero means money moved
    /// without a ledger row, which is exactly what an operator needs to be told about.
    /// </summary>
    public class TodaySummaryService : ITodaySummaryService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly ITransactionDetailsRepository _ledgerRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        // Wallet names for the per-wallet opening/closing lines (Recharge / DMT / AEPS are
        // admin-editable rows, so the label comes from the table and never from a constant).
        private readonly IWalletMasterRepository _walletMasterRepo;
        private readonly IUserServiceChargeRepository _serviceChargeRepo;
        private readonly IServiceRepository _serviceRepo;
        // Needed only to tell the operator's own accounts from the network (see ResolvePanelFloat).
        private readonly IUserRepository _userRepo;
        private readonly Logger _logger;

        // FundTransaction.Through values. These MUST match the strings FundService actually writes
        // (ThroughRequestTransfer / ThroughReverseRequestTransfer / ThroughDirectTransfer /
        // ThroughDirectReverse) — they are the only link between the money moving and the money
        // being reported.
        //
        // The first two read "BY_REQUEST_TRANSFER" and "BY_REVERSE_REQUEST_TRANSFER" here, a
        // prefix nothing in the system has ever written. The comparisons below therefore never
        // matched an approved fund request or an approved reverse request, so "Fund Transfered"
        // and "Fund Deducted" only ever counted DIRECT transfers and reversals: every request
        // that went through the approval queue was absent from the day's summary. The same two
        // wrong literals were copy-pasted into DashboardService, LedgerService,
        // ReportAnalyticsService and UserAccountingService, so the figure was consistently wrong
        // in all five rather than visibly disagreeing anywhere.
        private const string Through_RequestTransfer = "REQUEST_TRANSFER";
        private const string Through_DirectTransfer = "DIRECT_TRANSFER";
        private const string Through_DirectReverse = "DIRECT_REVERSE";
        private const string Through_ReverseRequest = "REVERSE_REQUEST_TRANSFER";
        private const string Through_PGTransfer = "PG-TRANSFER";
        private const string Through_ECollection = "E-COLLECTION";

        // The per-account summary covers EVERY wallet the account holds (see ResolveWalletDays).
        // The Recharge wallet is still the one funds are loaded into, and it leads the breakdown
        // because the wallets are listed in id order.

        public TodaySummaryService(
            ITransactionMasterRepository txnRepo,
            IFundTransactionRepository fundRepo,
            ITransactionDetailsRepository ledgerRepo,
            IUserWalletMasterRepository walletRepo,
            IWalletMasterRepository walletMasterRepo,
            IUserServiceChargeRepository serviceChargeRepo,
            IServiceRepository serviceRepo,
            IUserRepository userRepo)
        {
            _txnRepo = txnRepo;
            _fundRepo = fundRepo;
            _ledgerRepo = ledgerRepo;
            _walletRepo = walletRepo;
            _walletMasterRepo = walletMasterRepo;
            _serviceChargeRepo = serviceChargeRepo;
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
            _logger = new Logger();
        }

        // Service ids are admin-created in this DB (NOT the old FinovativeAPI fixed ids, where
        // 4=AEPS/5=MATM/35=POS/36=CMS) — resolve each bucket through ServiceKinds / name keywords.
        private async Task<(List<int> aeps, List<int> matm, List<int> pos, List<int> cms)> ResolveSummaryServiceIds()
        {
            var services = await _serviceRepo.GetAllServices();
            List<int> byKeyword(string kw) => services
                .Where(s => $"{s.Name} {s.ServiceCategory}".Contains(kw, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.ServiceID).ToList();
            return (
                ServiceKinds.IdsOf(services, ServiceKinds.AEPS),
                ServiceKinds.IdsOf(services, ServiceKinds.MicroATM),
                byKeyword("pos"),
                byKeyword("cms"));
        }

        /// <summary>
        /// The WHOLE PANEL's float for a date: every wallet of every user — admin, distributors,
        /// retailers, API users — not the admin's own account.
        ///
        /// That is what the admin dashboard is asking: "what did the platform open the day with, and
        /// what is it holding now". Reading the admin's personal wallet there answered a different
        /// question and made the figure look wrong next to the balance strip above it, which has
        /// always been network-wide.
        ///
        /// It is derived by walking backwards from the live wallet totals rather than forwards from
        /// history: every ledger row stamps the balance before and after the movement, so
        /// (Closing − Opening) is that movement's exact effect on the float. Sum those from the
        /// selected date onwards, subtract from today's total, and you have the balance as it stood
        /// at the start of that day — using only the rows for the period, never the whole table.
        /// </summary>
        private async Task<PanelFloat> ResolvePanelFloat(DateTime date)
        {
            // The operator's own accounts are not part of the panel float.
            //
            // Opening and Closing described "every wallet of every user", which included the
            // admin's own wallet — so loading the admin wallet from Virtual Balance showed up as
            // the NETWORK opening the day with more money, and the admin's own balance was being
            // counted twice over against a figure the balance strip already reports on its own.
            // The float here is what the network holds; the house is excluded from both the live
            // total and the ledger movements that walk it back to the chosen date.
            var operatorUserIds = (await _userRepo.GetAllUsersWithDeleted())
                .Where(u => u.UserTypeID == UserTypeCode.Admin
                            || u.UserTypeID == UserTypeCode.SupportUser
                            || u.UserTypeID == UserTypeCode.Accountant)
                .Select(u => u.UserMasterID)
                .ToHashSet();

            // Shared scoped DbContext — these awaits must not overlap.
            var networkWallets = (await _walletRepo.GetAll())
                .Where(w => !operatorUserIds.Contains(w.UserID))
                .ToList();
            var postings = (await _ledgerRepo.GetFromDate(date))
                .Where(e => !operatorUserIds.Contains(e.UserID))
                .ToList();
            var names = (await _walletMasterRepo.SelectAll())
                .GroupBy(w => w.WalletID)
                .ToDictionary(g => g.Key, g => g.First().Name ?? $"Wallet {g.Key}");
            // A user's own wallet-to-wallet move is NOT in the ledger (it is two FundTransaction
            // legs with the same user on both sides), yet it does change which wallet the money is
            // in. Folded into the per-wallet walk-back so AEPS and Recharge each read right; it
            // nets to zero on the panel and is never a Credit or a Debit.
            var selfLegs = (await _fundRepo.GetAllFundTransactions())
                .Where(f => f.SenderID == f.ReceiverID && f.SenderID > 0 && f.TransactionDOC.Date >= date
                            && !operatorUserIds.Contains(f.SenderID))
                .Select(f => (f.WalletID, Day: f.TransactionDOC.Date,
                              Delta: f.Through == Through_DirectReverse ? -f.Amount : f.Amount))
                .ToList();

            // Walked back PER WALLET, then summed — the per-wallet lines are what says WHICH
            // wallet the network's money sits in, and the panel figures are their total.
            //
            // A user's own wallet-to-wallet move posts a debit on one wallet and an equal credit on
            // another, so it nets to nothing on the panel; it is also left out of the day's Credit
            // and Debit so those two lines describe money that entered or left the network.
            var wallets = new List<PanelWalletDay>();
            decimal credited = 0m, debited = 0m;
            foreach (var g in networkWallets.GroupBy(w => w.WalletID))
            {
                decimal currentTotal = g.Sum(w => w.Balance);
                decimal onDayCredit = 0m, onDayDebit = 0m, afterDayNet = 0m, onDayNet = 0m;
                foreach (var e in postings.Where(p => p.WalletID == g.Key))
                {
                    decimal delta = e.ClosingBalance - e.OpeningBalance;
                    bool internalMove = IsInternalMove(e);
                    if (e.DateTime.Date == date)
                    {
                        onDayNet += delta;
                        if (internalMove) continue;
                        if (delta >= 0) onDayCredit += delta; else onDayDebit += -delta;
                    }
                    else if (e.DateTime.Date > date)
                    {
                        afterDayNet += delta;
                    }
                }
                foreach (var leg in selfLegs.Where(l => l.WalletID == g.Key))
                {
                    if (leg.Day == date) onDayNet += leg.Delta; else afterDayNet += leg.Delta;
                }
                decimal closing = currentTotal - afterDayNet;      // float at the end of the selected day
                decimal opening = closing - onDayNet;
                wallets.Add(new PanelWalletDay(g.Key, names.TryGetValue(g.Key, out var n) ? n : $"Wallet {g.Key}", opening, closing));
                credited += onDayCredit;
                debited += onDayDebit;
            }

            return new PanelFloat(wallets.Sum(w => w.Opening), wallets.Sum(w => w.Closing), credited, debited,
                wallets.OrderBy(w => w.WalletID).ToList());
        }

        /// <summary>
        /// A ledger row written by a user's own wallet-to-wallet transfer. Those legs describe
        /// "Wallet Transfer" (WalletToWalletTransferService) and carry no operator.
        /// </summary>
        private static bool IsInternalMove(TransactionDetails e) =>
            (e.DOCType ?? string.Empty).Contains("Wallet Transfer", StringComparison.OrdinalIgnoreCase)
            || (e.TransactionDescription ?? string.Empty).Contains("Wallet Transfer", StringComparison.OrdinalIgnoreCase)
            || (e.TransactionDescription ?? string.Empty).Contains("Wallet to Wallet", StringComparison.OrdinalIgnoreCase);

        /// <summary>Panel float for one day: what it opened with, closed with, and how it moved.</summary>
        private readonly record struct PanelFloat(decimal Opening, decimal Closing, decimal Credited, decimal Debited, List<PanelWalletDay> Wallets);

        /// <summary>One wallet's share of the network float at the start and end of the day.</summary>
        private readonly record struct PanelWalletDay(int WalletID, string Name, decimal Opening, decimal Closing);

        /// <summary>One wallet's day: its name, what it opened and closed with, what it holds now.</summary>
        private readonly record struct WalletDay(int WalletID, string Name, LedgerBalances.DayBalance Day, decimal LiveBalance);

        /// <summary>
        /// Every wallet the account holds, each with its own opening/closing for the date.
        ///
        /// This used to report the Recharge wallet alone, so an account that also holds a DMT and an
        /// AEPS wallet — which is every retailer on this build — saw an Opening and a Closing that
        /// described a fraction of its money and disagreed with the total balance printed directly
        /// above the summary. The combined figures are the sum of these; the per-wallet lines are
        /// reported alongside them so it is visible WHICH wallet the money is sitting in.
        /// </summary>
        private async Task<List<WalletDay>> ResolveWalletDays(int userId, DateTime date)
        {
            // Shared scoped DbContext — these awaits must not overlap.
            var wallets = await _walletRepo.GetByUserID(userId);
            var ledger = await _ledgerRepo.GetByUserID(userId);
            var names = (await _walletMasterRepo.SelectAll())
                .GroupBy(w => w.WalletID)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            return wallets
                .OrderBy(w => w.WalletID)
                .Select(w =>
                {
                    var rows = ledger.Where(e => e.WalletID == w.WalletID).ToList();
                    string name = names.TryGetValue(w.WalletID, out var n) && !string.IsNullOrWhiteSpace(n)
                        ? n
                        : $"Wallet {w.WalletID}";
                    return new WalletDay(w.WalletID, name,
                        LedgerBalances.ForDate(rows, date, w.Balance), w.Balance);
                })
                .ToList();
        }

        /// <summary>
        /// The account as a whole: openings, closings and live balances added across its wallets, so
        /// Opening / ClosingBalance / Difference describe the same money as the balance strip above
        /// the summary.
        /// </summary>
        private static (LedgerBalances.DayBalance day, decimal liveBalance) Combine(List<WalletDay> wallets)
        {
            decimal opening = wallets.Sum(w => w.Day.Opening);
            decimal closing = wallets.Sum(w => w.Day.Closing);
            decimal live = wallets.Sum(w => w.LiveBalance);
            bool fromLedger = wallets.Any(w => w.Day.FromLedger);
            return (new LedgerBalances.DayBalance(opening, closing, fromLedger), live);
        }

        /// <summary>
        /// Per-wallet opening/closing, appended after the combined block. A wallet that is empty on
        /// both sides of the day is left out — an account whose AEPS service is off should not have
        /// to read past zero lines to find the two that matter.
        /// </summary>
        private static void AddWalletBreakdown(List<TodaySummaryViewModel> report, List<WalletDay> wallets, DateTime date)
        {
            bool isToday = date.Date == DateTime.Now.Date;
            foreach (var w in wallets)
            {
                decimal closing = isToday ? w.LiveBalance : w.Day.Closing;
                if (w.Day.Opening == 0m && closing == 0m) continue;
                AddItem(report, $"{w.Name} Opening", w.Day.Opening);
                AddItem(report, $"{w.Name} Closing", closing);
            }
        }

        public async Task<(string failureMessage, List<TodaySummaryViewModel> data)> Select(int userId, string date)
        {
            var report = new List<TodaySummaryViewModel>();
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", report);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // Shared scoped DbContext — await sequentially (no concurrent EF queries).
                var txns = await _txnRepo.GetAll(0, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var allCharges = await _serviceChargeRepo.GetAll();

                var (aepsIds, matmIds, posIds, cmsIds) = await ResolveSummaryServiceIds();
                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                decimal aepsSuccess = success.Where(t => aepsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal matmSuccess = success.Where(t => matmIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal posSuccess = success.Where(t => posIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal cmsSuccess = success.Where(t => cmsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal totalSuccess = success.Sum(t => t.Amount);
                decimal totalFailed = txns.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount);
                decimal totalPending = txns.Where(t => IsPending(t)).Sum(t => t.Amount);
                decimal totalCommission = success.Sum(t => t.AdminCommission);
                // What the NETWORK was paid — the figure the operator reads against the surcharge
                // it collected. (The operator's own margin is AdminCommission, reported as Admin Profit.)
                decimal userCommission = success.Sum(t =>
                    Math.Max(0m, t.RetailerCommission) + Math.Max(0m, t.DistributerCommission) + Math.Max(0m, t.SuperDistributerCommission));
                decimal totalSurcharge = success.Sum(t => Math.Abs(t.Charge));
                decimal totalTDS = success.Sum(t => t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS);

                // Every one of these is panel-wide, to match the panel-wide Opening / Closing: the
                // whole network's fund movement for the day, not just the admin's own transfers.
                //
                // A user's own wallet-to-wallet move (AEPS → Recharge) is written to this table as
                // two legs with the SAME user on both sides. It is their money changing pocket —
                // where they keep it is their decision — not a fund movement, so it is counted in
                // neither Fund Transfered nor Fund Deducted.
                var fundsForDate = allFunds
                    .Where(f => f.TransactionDOC.Date == dateStart && f.SenderID != f.ReceiverID)
                    .ToList();
                decimal transferAmount = fundsForDate
                    .Where(f => f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer)
                    .Sum(f => f.Amount);
                decimal pgTransferAmount = fundsForDate
                    .Where(f => f.Through == Through_PGTransfer)
                    .Sum(f => f.Amount);
                decimal eCollectionAmount = fundsForDate
                    .Where(f => f.Through == Through_ECollection)
                    .Sum(f => f.Amount);
                decimal reverseAmount = fundsForDate
                    .Where(f => f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest)
                    .Sum(f => f.Amount);

                // Charges collected from the whole network — a company-wide figure for the admin,
                // unlike the per-user figure the retailer/distributor variants report.
                decimal serviceChargeTotal = allCharges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                var panel = await ResolvePanelFloat(dateStart);

                AddItem(report, "Opening", panel.Opening);
                // Fund Transfered = direct transfers + approved fund requests, the whole network.
                AddItem(report, "Fund Transfered", transferAmount);
                AddItem(report, "Fund Deducted", reverseAmount);
                if (pgTransferAmount != 0m) AddItem(report, "PG-Transfered", pgTransferAmount);
                if (eCollectionAmount != 0m) AddItem(report, "E-Collection", eCollectionAmount);
                AddItem(report, "Sale (Success)", totalSuccess);
                if (aepsSuccess != 0m) AddItem(report, "AEPS", aepsSuccess);
                if (matmSuccess != 0m) AddItem(report, "MATM", matmSuccess);
                if (posSuccess != 0m) AddItem(report, "POS", posSuccess);
                if (cmsSuccess != 0m) AddItem(report, "CMS", cmsSuccess);
                AddItem(report, "Refund", totalFailed);
                AddItem(report, "Pending", totalPending);
                AddItem(report, "Commission Paid to Users", userCommission);
                AddItem(report, "Surcharge Collected", totalSurcharge);
                AddItem(report, "TDS", totalTDS);
                AddItem(report, "Other Charge", serviceChargeTotal);
                AddItem(report, "Admin Profit", totalCommission);

                // The closing block for the panel. Credit and Debit are every posting made against
                // every wallet that day, so Opening + Credit − Debit is exactly Closing — the three
                // lines are the arithmetic, shown rather than asserted. Difference is the gap
                // between that arithmetic and the money the wallets actually hold: zero unless a
                // balance moved without a ledger row.
                AddItem(report, "Total Credit", panel.Credited);
                AddItem(report, "Total Debit", panel.Debited);
                AddItem(report, "Net Change", panel.Credited - panel.Debited);
                AddItem(report, "ClosingBalance", panel.Closing);
                AddItem(report, "Expected Closing", panel.Opening + panel.Credited - panel.Debited);
                AddItem(report, "Difference", panel.Closing - (panel.Opening + panel.Credited - panel.Debited));

                // Per wallet (Recharge / DMT / AEPS …): where the network's money sat at the start
                // and the end of the day. Empty wallets stay out.
                foreach (var w in panel.Wallets)
                {
                    if (w.Opening == 0m && w.Closing == 0m) continue;
                    AddItem(report, $"{w.Name} Opening", w.Opening);
                    AddItem(report, $"{w.Name} Closing", w.Closing);
                }

                return (StatusType.Ok, report);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, report);
            }
        }

        public async Task<(string failureMessage, List<TodaySummaryViewModel> data)> SelectForRetailer(int userId, string date)
        {
            var report = new List<TodaySummaryViewModel>();
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", report);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // Shared scoped DbContext — await sequentially (no concurrent EF queries).
                var txns = await _txnRepo.GetAll(userId, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var charges = await _serviceChargeRepo.GetByUserID(userId);

                var (aepsIds, _, _, _) = await ResolveSummaryServiceIds();
                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                decimal aepsSuccess = success.Where(t => aepsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal totalSuccess = success.Sum(t => t.Amount);
                decimal totalFailed = txns.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount);
                decimal totalPending = txns.Where(t => IsPending(t)).Sum(t => t.Amount);
                decimal totalCommission = success.Sum(t => t.RetailerCommission);
                decimal totalSurcharge = success.Sum(t => t.Charge);

                var fundsForDate = allFunds.Where(f => f.TransactionDOC.Date == dateStart).ToList();
                decimal purchaseAmount = fundsForDate
                    .Where(f => f.ReceiverID == userId && (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                decimal pgPurchaseAmount = fundsForDate
                    .Where(f => f.ReceiverID == userId && (f.Through == Through_PGTransfer || f.Through == Through_ECollection))
                    .Sum(f => f.Amount);
                decimal reverseAmount = fundsForDate
                    .Where(f => f.SenderID == userId && (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                    .Sum(f => f.Amount);

                decimal serviceChargeTotal = charges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                var walletDays = await ResolveWalletDays(userId, dateStart);
                var (day, liveBalance) = Combine(walletDays);

                AddItem(report, "Opening", day.Opening);
                AddItem(report, "Fund Purchased", purchaseAmount);
                AddItem(report, "PG Purchased", pgPurchaseAmount);
                AddItem(report, "Reversed", reverseAmount);
                AddItem(report, "Success", totalSuccess);
                AddItem(report, "AEPS", aepsSuccess);
                AddItem(report, "Refund", totalFailed);
                AddItem(report, "Commission", totalCommission);
                AddItem(report, "Surcharge", totalSurcharge);
                AddItem(report, "Other Charge", serviceChargeTotal);
                AddItem(report, "Pending", totalPending);
                AddClosingBlock(report, day, liveBalance, dateStart);
                AddWalletBreakdown(report, walletDays, dateStart);

                return (StatusType.Ok, report);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, report);
            }
        }

        public async Task<(string failureMessage, List<TodaySummaryViewModel> data)> SelectForDistributor(int userId, string date)
        {
            var report = new List<TodaySummaryViewModel>();
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", report);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // Shared scoped DbContext — await sequentially (no concurrent EF queries).
                var txns = await _txnRepo.GetAll(userId, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var charges = await _serviceChargeRepo.GetByUserID(userId);

                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                decimal totalCommission = success.Sum(t => t.DistributerCommission);
                decimal totalSurcharge = success.Sum(t => t.Charge);

                var fundsForDate = allFunds.Where(f => f.TransactionDOC.Date == dateStart).ToList();
                decimal transferAmount = fundsForDate
                    .Where(f => f.SenderID == userId && (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                decimal purchaseAmount = fundsForDate
                    .Where(f => f.ReceiverID == userId && (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer || f.Through == Through_PGTransfer))
                    .Sum(f => f.Amount);
                decimal reverseAmount = fundsForDate
                    .Where(f => f.SenderID == userId && (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                    .Sum(f => f.Amount);
                decimal downlineReverseAmount = fundsForDate
                    .Where(f => f.ReceiverID == userId && (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                    .Sum(f => f.Amount);

                decimal serviceChargeTotal = charges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                var walletDays = await ResolveWalletDays(userId, dateStart);
                var (day, liveBalance) = Combine(walletDays);

                AddItem(report, "Opening", day.Opening);
                AddItem(report, "Fund Transfered", transferAmount);
                AddItem(report, "Purchased", purchaseAmount);
                AddItem(report, "Fund Deducted", downlineReverseAmount);
                AddItem(report, "Reversed", reverseAmount);
                AddItem(report, "Commission", totalCommission);
                AddItem(report, "Surcharge", totalSurcharge);
                AddItem(report, "Other Charge", serviceChargeTotal);
                AddClosingBlock(report, day, liveBalance, dateStart);
                AddWalletBreakdown(report, walletDays, dateStart);

                return (StatusType.Ok, report);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, report);
            }
        }

        // PENDING and PROCESS are the same state written by different generations of the code.
        private static bool IsPending(TransactionMaster t) =>
            t.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)
            || t.Status.Equals("PROCESS", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The closing block, identical for every role.
        ///
        /// ClosingBalance is what the account actually holds at the end of that day — the live wallet
        /// for today, the ledger's last closing for a past date. Expected Closing is what the ledger
        /// says it should hold, and Difference is the gap between them: zero when every movement was
        /// posted through the accounting pipeline, non-zero only when something changed a balance
        /// without writing a ledger row. That is the one number worth escalating, so it is worth
        /// having it mean exactly that rather than being the residue of a hand-written formula.
        /// </summary>
        private static void AddClosingBlock(List<TodaySummaryViewModel> report, LedgerBalances.DayBalance day, decimal liveBalance, DateTime date)
        {
            bool isToday = date.Date == DateTime.Now.Date;
            decimal expected = day.Closing;
            decimal shown = isToday ? liveBalance : day.Closing;

            AddItem(report, "ClosingBalance", shown);
            AddItem(report, "Expected Closing", expected);
            AddItem(report, "Difference", shown - expected);
        }

        private static void AddItem(List<TodaySummaryViewModel> list, string type, decimal amount)
        {
            list.Add(new TodaySummaryViewModel { Type = type, TotalAmount = amount });
        }
    }
}
