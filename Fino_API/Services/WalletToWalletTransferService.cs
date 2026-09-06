using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.WalletToWalletTransfer;
using FintechApp.Responses.WalletToWalletTransfer;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old WalletToWalletTransferBal.Transfer — moves funds from the user's
    // AEPS wallet to another of their own wallets (internal, no external provider).
    public class WalletToWalletTransferService : IWalletToWalletTransferService
    {
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IUserBalanceCappingRepository _cappingRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly Logger _logger;

        private const string Through_DirectReverse = "DIRECT_REVERSE";
        private const string Through_DirectTransfer = "DIRECT_TRANSFER";
        private const string Type_Transfer = "TRANSFER";
        private const string PayMode_Cash = "CASH";

        public WalletToWalletTransferService(
            IUserWalletMasterRepository userWalletRepo,
            IFundTransactionRepository fundRepo,
            IUserBalanceCappingRepository cappingRepo,
            IWalletMasterRepository walletRepo)
        {
            _userWalletRepo = userWalletRepo;
            _fundRepo = fundRepo;
            _cappingRepo = cappingRepo;
            _walletRepo = walletRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, WalletTransferResponse? data)> AEPSToOther(AEPSToOtherRequest request, int currentUserID)
        {
            try
            {
                if (request.WalletID <= 0 || request.Amount <= 0)
                    return (Messages.Common.Message.InvalidRequest, null);

                int fromWalletID = (int)WalletCode.AEPS;

                if (fromWalletID == request.WalletID)
                    return (Messages.Wallet.Message.SameWalletTransfer, null);

                // Target wallet must exist and be active
                var walletMaster = await _walletRepo.SelectByID(request.WalletID);
                if (walletMaster == null)
                    return (Messages.Wallet.Message.DestinationNotFound, null);
                if (!walletMaster.IsActive)
                    return (Messages.Wallet.Message.TransferNotAllowed, null);

                // Load the user's wallets
                var userWallets = await _userWalletRepo.GetByUserID(currentUserID);
                var fromWallet = userWallets.FirstOrDefault(w => w.WalletID == fromWalletID);
                var toWallet = userWallets.FirstOrDefault(w => w.WalletID == request.WalletID);

                if (fromWallet == null)
                    return (Messages.Wallet.Message.NotFound, null);
                if (toWallet == null)
                    return (Messages.Wallet.Message.DestinationNotFound, null);

                // Sufficient balance
                if (request.Amount > fromWallet.Balance)
                    return (Messages.Wallet.Message.InsufficientBalance, null);

                // Minimum-balance capping on AEPS wallet
                var cappings = await _cappingRepo.GetByUserID(currentUserID);
                decimal cappingAmount = cappings
                    .Where(c => c.WalletID == fromWalletID && !c.IsDelete)
                    .Select(c => c.Amount)
                    .FirstOrDefault();

                if ((fromWallet.Balance - cappingAmount) < request.Amount)
                    return (Messages.Wallet.Message.MinimumBalanceRequired, null);

                // Move the money atomically: debit the AEPS wallet (respecting any held portion), then
                // credit the destination wallet. Each side is a single conditional UPDATE, so this is
                // race-safe under concurrency — no read-modify-write lost update.
                var deb = await _userWalletRepo.TryApplyDelta(currentUserID, fromWalletID, -request.Amount,
                    allowOverdraft: false, respectHold: true);
                if (!deb.Applied)
                    return (Messages.Wallet.Message.InsufficientBalance, null);

                var cred = await _userWalletRepo.TryApplyDelta(currentUserID, request.WalletID, request.Amount,
                    allowOverdraft: false, respectHold: false);
                if (!cred.Applied)
                {
                    // Destination credit failed — undo the debit so no money is lost.
                    await _userWalletRepo.TryApplyDelta(currentUserID, fromWalletID, request.Amount, allowOverdraft: true, respectHold: false);
                    return (Messages.Wallet.Message.DestinationNotFound, null);
                }

                decimal fromOpening = deb.OpeningBalance;
                decimal fromClosing = deb.ClosingBalance;
                decimal toOpening = cred.OpeningBalance;
                decimal toClosing = cred.ClosingBalance;

                DateTime now = DateTime.Now;

                // Leg 1 — reverse out of AEPS wallet
                var reverseLeg = BuildFundTransaction(
                    walletID: fromWalletID,
                    currentUserID: currentUserID,
                    amount: request.Amount,
                    fromOpening: fromOpening, fromClosing: fromClosing,
                    toOpening: toOpening, toClosing: toClosing,
                    remark: request.Remark,
                    through: Through_DirectReverse,
                    now: now);
                await _fundRepo.Insert(reverseLeg);

                // Leg 2 — transfer into destination wallet
                var transferLeg = BuildFundTransaction(
                    walletID: request.WalletID,
                    currentUserID: currentUserID,
                    amount: request.Amount,
                    fromOpening: fromOpening, fromClosing: fromClosing,
                    toOpening: toOpening, toClosing: toClosing,
                    remark: request.Remark,
                    through: Through_DirectTransfer,
                    now: now);
                await _fundRepo.Insert(transferLeg);

                var response = new WalletTransferResponse
                {
                    AepsWalletBal = fromClosing.ToString("0.00"),
                    mainWalletBal = toClosing.ToString("0.00"),
                    ReferenceID = transferLeg.FundTransactionID,
                    Remark = request.Remark,
                    Amount = request.Amount,
                    DateTime = now.ToString("dd/MM/yyyy hh:mm tt"),
                    Message = Messages.Fund.Message.TransferSuccess
                };

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        private static FundTransaction BuildFundTransaction(
            int walletID, int currentUserID, decimal amount,
            decimal fromOpening, decimal fromClosing, decimal toOpening, decimal toClosing,
            string remark, string through, DateTime now)
        {
            return new FundTransaction
            {
                Amount = amount,
                OriginalAmount = amount,
                WalletID = walletID,
                ReceiverID = currentUserID,
                SenderID = currentUserID,
                RequestToID = currentUserID,
                RequestAcceptedUserID = currentUserID,
                SenderOpeningBalance = fromOpening,
                SenderClosingBalance = fromClosing,
                ReceiverOpeningBalance = toOpening,
                ReceiverClosingBalance = toClosing,
                AdminRemark = remark ?? string.Empty,
                Remark = string.IsNullOrWhiteSpace(remark) ? "Wallet Transfer" : remark,
                PayMode = PayMode_Cash,
                Type = Type_Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = string.Empty,
                UTRNumber = string.Empty,
                Description = "AEPS wallet to other wallet transfer",
                BankName = string.Empty,
                ReceiptPath = string.Empty,
                Through = through,
                TransactionDOC = now,
                TransactionDOCInt = int.Parse(now.ToString("yyyyMMdd")),
                IsDelete = false,
                DateTime = now,
                UpdatedateTime = now
            };
        }
    }
}
