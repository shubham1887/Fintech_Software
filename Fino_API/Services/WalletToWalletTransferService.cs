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

        /// <summary>
        /// Legacy entry point: AEPS wallet → another of the caller's wallets. Kept because the
        /// Android build and the old panel call it by this name; it is now a thin wrapper over the
        /// general transfer so there is one implementation of moving money between wallets.
        /// </summary>
        public Task<(string failureMessage, WalletTransferResponse? data)> AEPSToOther(AEPSToOtherRequest request, int currentUserID)
            => Transfer(new WalletTransferRequest
            {
                FromWalletID = (int)WalletCode.AEPS,
                ToWalletID = request.WalletID,
                Amount = request.Amount,
                Remark = request.Remark,
            }, currentUserID);

        /// <summary>
        /// Move money between two of the caller's OWN wallets, in either direction.
        ///
        /// Everything here is deliberately the caller's own money: both wallet ids are looked up in
        /// the caller's wallet list, so a wallet belonging to somebody else is simply "not found"
        /// rather than a source of funds. The debit and the credit are each one conditional UPDATE
        /// (TryApplyDelta), so two transfers racing cannot overdraw a wallet, and a failed credit
        /// puts the debit straight back — money is never left in neither wallet.
        ///
        /// Both legs are written to FundTransaction exactly as the AEPS transfer always did, so the
        /// movement shows up in the ledger, the daybook and the fund report with no special case.
        /// </summary>
        public async Task<(string failureMessage, WalletTransferResponse? data)> Transfer(WalletTransferRequest request, int currentUserID)
        {
            try
            {
                if (request.FromWalletID <= 0 || request.ToWalletID <= 0 || request.Amount <= 0)
                    return (Messages.Common.Message.InvalidRequest, null);

                if (request.FromWalletID == request.ToWalletID)
                    return (Messages.Wallet.Message.SameWalletTransfer, null);

                // This install offers ONE movement: AEPS earnings out into another wallet. Free
                // wallet-to-wallet is deliberately not on the menu — a retailer shifting money
                // back and forth between Recharge and DMT at will is not something the operator
                // wants to support or reconcile. The rule lives here rather than only in the
                // screen, so the endpoint cannot be driven past the UI either. To allow the
                // general case again, delete this check; everything below already handles any
                // pair of the caller's own wallets.
                if (request.FromWalletID != (int)WalletCode.AEPS)
                    return ("Only the AEPS wallet can be transferred from.", null);

                // Both wallets must exist and be switched on. A disabled wallet is not a destination:
                // money parked in one the admin has turned off cannot be spent or moved out again.
                var fromMaster = await _walletRepo.SelectByID(request.FromWalletID);
                var toMaster = await _walletRepo.SelectByID(request.ToWalletID);
                if (fromMaster == null) return (Messages.Wallet.Message.NotFound, null);
                if (toMaster == null) return (Messages.Wallet.Message.DestinationNotFound, null);
                if (!fromMaster.IsActive || !toMaster.IsActive)
                    return (Messages.Wallet.Message.TransferNotAllowed, null);

                // The caller's own wallets — this is what stops one user moving another's money.
                var userWallets = await _userWalletRepo.GetByUserID(currentUserID);
                var fromWallet = userWallets.FirstOrDefault(w => w.WalletID == request.FromWalletID);
                var toWallet = userWallets.FirstOrDefault(w => w.WalletID == request.ToWalletID);

                if (fromWallet == null) return (Messages.Wallet.Message.NotFound, null);
                if (toWallet == null) return (Messages.Wallet.Message.DestinationNotFound, null);

                if (request.Amount > fromWallet.Balance)
                    return (Messages.Wallet.Message.InsufficientBalance, null);

                // Minimum-balance capping on the SOURCE wallet (admin can pin a floor per wallet).
                var cappings = await _cappingRepo.GetByUserID(currentUserID);
                decimal cappingAmount = cappings
                    .Where(c => c.WalletID == request.FromWalletID && !c.IsDelete)
                    .Select(c => c.Amount)
                    .FirstOrDefault();

                if ((fromWallet.Balance - cappingAmount) < request.Amount)
                    return (Messages.Wallet.Message.MinimumBalanceRequired, null);

                // Debit respects any held portion; the credit does not need to.
                var deb = await _userWalletRepo.TryApplyDelta(currentUserID, request.FromWalletID, -request.Amount,
                    allowOverdraft: false, respectHold: true);
                if (!deb.Applied)
                    return (Messages.Wallet.Message.InsufficientBalance, null);

                var cred = await _userWalletRepo.TryApplyDelta(currentUserID, request.ToWalletID, request.Amount,
                    allowOverdraft: false, respectHold: false);
                if (!cred.Applied)
                {
                    // Destination credit failed — undo the debit so no money is lost.
                    await _userWalletRepo.TryApplyDelta(currentUserID, request.FromWalletID, request.Amount,
                        allowOverdraft: true, respectHold: false);
                    return (Messages.Wallet.Message.DestinationNotFound, null);
                }

                decimal fromOpening = deb.OpeningBalance;
                decimal fromClosing = deb.ClosingBalance;
                decimal toOpening = cred.OpeningBalance;
                decimal toClosing = cred.ClosingBalance;

                DateTime now = DateTime.Now;
                string description = $"{fromMaster.Name} wallet to {toMaster.Name} wallet transfer";

                // Leg 1 — money leaving the source wallet
                var reverseLeg = BuildFundTransaction(
                    walletID: request.FromWalletID,
                    currentUserID: currentUserID,
                    amount: request.Amount,
                    fromOpening: fromOpening, fromClosing: fromClosing,
                    toOpening: toOpening, toClosing: toClosing,
                    remark: request.Remark,
                    through: Through_DirectReverse,
                    description: description,
                    now: now);
                await _fundRepo.Insert(reverseLeg);

                // Leg 2 — money arriving in the destination wallet
                var transferLeg = BuildFundTransaction(
                    walletID: request.ToWalletID,
                    currentUserID: currentUserID,
                    amount: request.Amount,
                    fromOpening: fromOpening, fromClosing: fromClosing,
                    toOpening: toOpening, toClosing: toClosing,
                    remark: request.Remark,
                    through: Through_DirectTransfer,
                    description: description,
                    now: now);
                await _fundRepo.Insert(transferLeg);

                var response = new WalletTransferResponse
                {
                    AepsWalletBal = fromClosing.ToString("0.00"),
                    mainWalletBal = toClosing.ToString("0.00"),
                    FromWalletName = fromMaster.Name,
                    ToWalletName = toMaster.Name,
                    FromWalletBalance = fromClosing,
                    ToWalletBalance = toClosing,
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

        /// <summary>
        /// The caller's own wallets, newest balance first-hand from the wallet table. Only active
        /// wallets are offered: a transfer into a disabled wallet is refused anyway, and offering it
        /// in the picker only invites the error.
        /// </summary>
        public async Task<List<MyWalletResponse>> MyWallets(int currentUserID)
        {
            var masters = (await _walletRepo.SelectAll())
                .GroupBy(w => w.WalletID)
                .ToDictionary(g => g.Key, g => g.First());

            var rows = new List<MyWalletResponse>();
            foreach (var w in (await _userWalletRepo.GetByUserID(currentUserID)).OrderBy(w => w.WalletID))
            {
                if (!masters.TryGetValue(w.WalletID, out var master) || !master.IsActive) continue;
                rows.Add(new MyWalletResponse
                {
                    WalletID = w.WalletID,
                    Name = master.Name,
                    ShortName = master.ShortName ?? string.Empty,
                    Balance = w.Balance,
                    HoldBalance = w.HoldBalance,
                    Label = $"{master.Name} — ₹{w.Balance:N2}",
                });
            }
            return rows;
        }

        /// <inheritdoc />
        public async Task<List<MyWalletResponse>> AEPSDestinations(int currentUserID)
            => (await MyWallets(currentUserID)).Where(w => w.WalletID != (int)WalletCode.AEPS).ToList();

        private static FundTransaction BuildFundTransaction(
            int walletID, int currentUserID, decimal amount,
            decimal fromOpening, decimal fromClosing, decimal toOpening, decimal toClosing,
            string remark, string through, string description, DateTime now)
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
                Description = description,
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
