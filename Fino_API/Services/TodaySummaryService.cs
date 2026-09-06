using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.TodaySummary;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TodaySummaryService : ITodaySummaryService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IUserBalanceHistoryRepository _balHistoryRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IUserServiceChargeRepository _serviceChargeRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly Logger _logger;

        private const string Through_RequestTransfer = "BY_REQUEST_TRANSFER";
        private const string Through_DirectTransfer = "DIRECT_TRANSFER";
        private const string Through_DirectReverse = "DIRECT_REVERSE";
        private const string Through_ReverseRequest = "BY_REVERSE_REQUEST_TRANSFER";
        private const string Through_PGTransfer = "PG-TRANSFER";
        private const string Through_ECollection = "E-COLLECTION";

        public TodaySummaryService(
            ITransactionMasterRepository txnRepo,
            IFundTransactionRepository fundRepo,
            IUserBalanceHistoryRepository balHistoryRepo,
            IUserWalletMasterRepository walletRepo,
            IUserServiceChargeRepository serviceChargeRepo,
            IServiceRepository serviceRepo)
        {
            _txnRepo = txnRepo;
            _fundRepo = fundRepo;
            _balHistoryRepo = balHistoryRepo;
            _walletRepo = walletRepo;
            _serviceChargeRepo = serviceChargeRepo;
            _serviceRepo = serviceRepo;
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
                var allBalHist = await _balHistoryRepo.GetAll();
                var allWallets = await _walletRepo.GetAll();
                var allCharges = await _serviceChargeRepo.GetAll();

                var (aepsIds, matmIds, posIds, cmsIds) = await ResolveSummaryServiceIds();
                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                decimal aepsSuccess = success.Where(t => aepsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal matmSuccess = success.Where(t => matmIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal posSuccess = success.Where(t => posIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal cmsSuccess = success.Where(t => cmsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal totalSuccess = success.Sum(t => t.Amount);
                decimal totalFailed = txns.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount);
                decimal totalPending = txns.Where(t => t.Status == "PROCESS").Sum(t => t.Amount);
                decimal totalCommission = success.Sum(t => t.AdminCommission);
                decimal totalSurcharge = success.Sum(t => t.Charge);
                decimal totalTDS = success.Sum(t => t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS);

                var fundsForDate = allFunds.Where(f => f.TransactionDOC.Date == dateStart).ToList();
                decimal transferAmount = fundsForDate
                    .Where(f => f.SenderID == userId && (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                decimal pgTransferAmount = fundsForDate
                    .Where(f => f.SenderID == userId && f.Through == Through_PGTransfer)
                    .Sum(f => f.Amount);
                decimal eCollectionAmount = fundsForDate
                    .Where(f => f.SenderID == userId && f.Through == Through_ECollection)
                    .Sum(f => f.Amount);
                decimal reverseAmount = fundsForDate
                    .Where(f => f.ReceiverID == userId && (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                    .Sum(f => f.Amount);

                var openingHist = allBalHist
                    .Where(b => b.DateTime.Date == dateStart)
                    .OrderBy(b => b.DateTime)
                    .FirstOrDefault();
                decimal openingBalance = openingHist?.Balance ?? 0;

                decimal closingBalance = allWallets
                    .Where(w => w.WalletID == 1)
                    .Sum(w => w.Balance);

                decimal serviceChargeTotal = allCharges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                decimal nonCashSuccess = totalSuccess - (aepsSuccess + matmSuccess + posSuccess);
                decimal expectedClosing = (openingBalance + transferAmount + pgTransferAmount + eCollectionAmount + totalCommission + aepsSuccess + matmSuccess + posSuccess)
                    - (nonCashSuccess + reverseAmount + totalSurcharge + serviceChargeTotal + totalTDS);
                decimal difference = closingBalance - expectedClosing;

                AddItem(report, "Opening", openingBalance);
                AddItem(report, "Fund Transfered", transferAmount);
                AddItem(report, "AEPS", aepsSuccess);
                AddItem(report, "MATM", matmSuccess);
                AddItem(report, "POS", posSuccess);
                AddItem(report, "CMS", cmsSuccess);
                AddItem(report, "PG-Transfered", pgTransferAmount);
                AddItem(report, "E-Collection", eCollectionAmount);
                AddItem(report, "Fund Deducted", reverseAmount);
                AddItem(report, "Success", totalSuccess);
                AddItem(report, "Refund", totalFailed);
                AddItem(report, "Commission", totalCommission);
                AddItem(report, "TDS", totalTDS);
                AddItem(report, "Surcharge", totalSurcharge);
                AddItem(report, "Other Charge", serviceChargeTotal);
                AddItem(report, "Pending", totalPending);
                AddItem(report, "ClosingBalance", closingBalance);
                AddItem(report, "Expected Closing", expectedClosing);
                AddItem(report, "Difference", difference + totalPending);

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
                var balHist = await _balHistoryRepo.GetByUserID(userId);
                var wallets = await _walletRepo.GetByUserID(userId);
                var charges = await _serviceChargeRepo.GetByUserID(userId);

                var (aepsIds, _, _, _) = await ResolveSummaryServiceIds();
                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                decimal aepsSuccess = success.Where(t => aepsIds.Contains(t.ServiceID)).Sum(t => t.Amount);
                decimal totalSuccess = success.Sum(t => t.Amount);
                decimal totalFailed = txns.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount);
                decimal totalPending = txns.Where(t => t.Status == "PROCESS").Sum(t => t.Amount);
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

                var openingHist = balHist
                    .Where(b => b.DateTime.Date == dateStart)
                    .OrderBy(b => b.DateTime)
                    .FirstOrDefault();
                decimal openingBalance = openingHist?.Balance ?? 0;

                decimal closingBalance = wallets.Where(w => w.WalletID == 1).Sum(w => w.Balance);

                decimal serviceChargeTotal = charges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                decimal expectedClosing = (openingBalance + purchaseAmount + totalCommission)
                    - (totalSuccess + totalSurcharge + serviceChargeTotal + reverseAmount);
                decimal difference = closingBalance - expectedClosing;

                AddItem(report, "Opening", openingBalance);
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
                AddItem(report, "ClosingBalance", closingBalance);
                AddItem(report, "Expected Closing", expectedClosing);
                AddItem(report, "Difference", difference);

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
                var balHist = await _balHistoryRepo.GetByUserID(userId);
                var wallets = await _walletRepo.GetByUserID(userId);
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

                var openingHist = balHist
                    .Where(b => b.DateTime.Date == dateStart)
                    .OrderBy(b => b.DateTime)
                    .FirstOrDefault();
                decimal openingBalance = openingHist?.Balance ?? 0;

                decimal closingBalance = wallets.Where(w => w.WalletID == 1).Sum(w => w.Balance);

                decimal serviceChargeTotal = charges
                    .Where(c => c.DateTime.Date == dateStart)
                    .Sum(c => c.Charge);

                decimal expectedClosing = (openingBalance + purchaseAmount + downlineReverseAmount + totalCommission)
                    - (transferAmount + reverseAmount + totalSurcharge + serviceChargeTotal);
                decimal difference = closingBalance - expectedClosing;

                AddItem(report, "Opening", openingBalance);
                AddItem(report, "Fund Transfered", transferAmount);
                AddItem(report, "Purchased", purchaseAmount);
                AddItem(report, "Fund Deducted", downlineReverseAmount);
                AddItem(report, "Reversed", reverseAmount);
                AddItem(report, "Commission", totalCommission);
                AddItem(report, "Surcharge", totalSurcharge);
                AddItem(report, "Other Charge", serviceChargeTotal);
                AddItem(report, "Closing Balance", closingBalance);
                AddItem(report, "Expected Closing", expectedClosing);
                AddItem(report, "Difference", difference);

                return (StatusType.Ok, report);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, report);
            }
        }

        private static void AddItem(List<TodaySummaryViewModel> list, string type, decimal amount)
        {
            list.Add(new TodaySummaryViewModel { Type = type, TotalAmount = amount });
        }
    }
}
