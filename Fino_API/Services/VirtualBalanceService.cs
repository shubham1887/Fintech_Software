using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AccountProcessing;
using FintechApp.Requests.VirtualBalance;
using FintechApp.Responses.VirtualBalance;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class VirtualBalanceService : IVirtualBalanceService
    {
        private readonly IVirtualBalanceRepository _repository;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly IAccountProcessingService _accountProcessing;
        private readonly ISettlementAccountingService _settlementAccounting;
        private readonly Logger _logger;

        public VirtualBalanceService(
            IVirtualBalanceRepository repository,
            IUserWalletMasterRepository userWalletRepo,
            IWalletMasterRepository walletRepo,
            IAccountProcessingService accountProcessing,
            ISettlementAccountingService settlementAccounting)
        {
            _repository = repository;
            _userWalletRepo = userWalletRepo;
            _walletRepo = walletRepo;
            _accountProcessing = accountProcessing;
            _settlementAccounting = settlementAccounting;
            _logger = new Logger();
        }

        private static VirtualBalanceResponse MapToResponse(VirtualBalance e) => new() { VirtualBalanceID = e.VirtualBalanceID, UserID = e.UserID, Amount = e.Amount, WalletID = e.WalletID, DOCType = e.DOCType, OpeningBalance = e.OpeningBalance, ClosingBalance = e.ClosingBalance, DateTime = e.DateTime, };

        // SelectAll (not Select) so a disabled wallet still resolves to a name on historic rows.
        private async Task<List<VirtualBalanceResponse>> WithWalletNames(List<VirtualBalance> list)
        {
            var nameById = (await _walletRepo.SelectAll()).ToDictionary(w => w.WalletID, w => w.Name);
            return list.Select(e =>
            {
                var r = MapToResponse(e);
                r.WalletName = nameById.TryGetValue(e.WalletID, out var n) ? n : string.Empty;
                return r;
            }).ToList();
        }

        public async Task<List<VirtualBalanceResponse>> GetAll() { var list = await _repository.GetAll(); return await WithWalletNames(list); }
        public async Task<VirtualBalanceResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : (await WithWalletNames(new List<VirtualBalance> { e })).FirstOrDefault(); }

        // Admin takes virtual balance for themselves: the amount is credited into the admin's own
        // UserWalletMaster row for WalletID (creating the row if it doesn't exist yet), and a
        // VirtualBalance ledger record is written. From this wallet the admin funds everyone else.
        public async Task<string> Create(CreateVirtualBalanceRequest request, int userID)
        {
            if (request.Amount <= 0) return Messages.ServiceMessages.InvalidValue;

            // Ensure the admin has a wallet row for this wallet; CreditBalance credits the existing row.
            var adminWallet = (await _userWalletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == request.WalletID);
            if (adminWallet == null)
            {
                await _userWalletRepo.Insert(new UserWalletMaster { UserID = userID, WalletID = request.WalletID, Balance = 0m, BalanceHash = null });
            }

            var posting = new AccountPostingRequest
            {
                UserID = userID,
                WalletID = request.WalletID,
                AmountWithCommission = request.Amount,
                Amount = request.Amount,
                DOCType = DOCTypeConstants.VirtualBalance,
                AssignedUserID = userID,
                TransactionDescription = "Virtual balance top-up",
            };

            int rows = await _accountProcessing.CreditBalance(posting);
            if (rows <= 0) return Messages.Common.Message.ExceptionOccured;

            // Balances come back from the posting itself. Re-reading the wallet here would race
            // with any concurrent debit against the same admin float wallet and record an opening
            // balance that disagrees with the ledger row CreditBalance just wrote.
            var entity = new VirtualBalance
            {
                UserID = userID,
                Amount = request.Amount,
                WalletID = request.WalletID,
                DOCType = DOCTypeConstants.VirtualBalance,
                OpeningBalance = posting.PostedOpeningBalance,
                ClosingBalance = posting.PostedClosingBalance,
                DateTime = DateTime.Now,
            };
            await _repository.Insert(entity);

            // Float arriving from outside is the one wallet credit that is real money in: Dr Bank,
            // Cr User Wallet control. A transfer between two users posts nothing, because both
            // sides of it sit inside the wallet control account and net to zero.
            await _settlementAccounting.OnWalletToppedUp(
                entity.VirtualBalanceID, entity.WalletID, entity.Amount, entity.DateTime);

            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateVirtualBalanceRequest request) { var entity = await _repository.GetByID(request.VirtualBalanceID); if (entity == null) return Messages.VirtualBalanceMessages.NotFound; entity.Amount = request.Amount; entity.WalletID = request.WalletID; entity.DOCType = request.DOCType; entity.OpeningBalance = request.OpeningBalance; entity.ClosingBalance = request.ClosingBalance; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.VirtualBalanceMessages.NotFound; }
    }
}
