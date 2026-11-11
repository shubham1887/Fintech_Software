using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderPurchase;
using FintechApp.Responses.ProviderPurchase;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderPurchaseService : IProviderPurchaseService
    {
        private readonly IProviderPurchaseRepository _repository;
        private readonly ISettlementAccountingService _settlementAccounting;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IWalletMasterRepository _walletRepo;

        public ProviderPurchaseService(
            IProviderPurchaseRepository repository,
            ISettlementAccountingService settlementAccounting,
            IProviderMasterRepository providerRepo,
            IWalletMasterRepository walletRepo)
        {
            _repository = repository;
            _settlementAccounting = settlementAccounting;
            _providerRepo = providerRepo;
            _walletRepo = walletRepo;
        }

        private static ProviderPurchaseResponse MapToResponse(ProviderPurchase e) => new()
        {
            ProviderPurchaseID = e.ProviderPurchaseID,
            ProviderID = e.ProviderID,
            WalletID = e.WalletID,
            Amount = e.Amount,
            Remark = e.Remark,
            PurchaseDate = e.PurchaseDate,
            PurchaseDOCInt = e.PurchaseDOCInt,
            IsDelete = e.IsDelete,
            DateTime = e.DateTime,
        };

        /// <summary>
        /// Names beside the ids. The list screen showed "Provider ID 3, Wallet ID 1", which is a
        /// database detail nobody recording a stock top-up can read; the provider and the wallet
        /// are what the row IS.
        /// </summary>
        private async Task<List<ProviderPurchaseResponse>> WithNames(IEnumerable<ProviderPurchase> rows)
        {
            var providers = (await _providerRepo.GetAll()).GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
            var wallets = (await _walletRepo.SelectAll()).GroupBy(w => w.WalletID).ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
            return rows.OrderByDescending(r => r.PurchaseDate).ThenByDescending(r => r.ProviderPurchaseID).Select(e =>
            {
                var r = MapToResponse(e);
                r.ProviderName = providers.TryGetValue(e.ProviderID, out var pn) ? pn : $"Provider {e.ProviderID}";
                r.WalletName = wallets.TryGetValue(e.WalletID, out var wn) ? wn : $"Wallet {e.WalletID}";
                return r;
            }).ToList();
        }

        public async Task<List<ProviderPurchaseResponse>> GetAll() { var list = await _repository.GetAll(); return await WithNames(list); }
        public async Task<ProviderPurchaseResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : (await WithNames(new[] { e })).FirstOrDefault(); }
        public async Task<List<ProviderPurchaseResponse>> GetByProviderID(int providerid) { var list = await _repository.GetByProviderID(providerid); return await WithNames(list); }
        public async Task<string> Create(CreateProviderPurchaseRequest request)
        {
            var entity = new ProviderPurchase
            {
                ProviderID = request.ProviderID,
                WalletID = request.WalletID,
                Amount = request.Amount,
                Remark = request.Remark,
                PurchaseDate = request.PurchaseDate,
                PurchaseDOCInt = request.PurchaseDOCInt,
                IsDelete = request.IsDelete,
            };
            await _repository.Insert(entity);

            // Stock bought from a provider: Dr Provider Stock, Cr Bank. This is the other half of
            // the provider-stock control account the daily integrity check ties out.
            await _settlementAccounting.OnProviderPurchase(
                entity.ProviderPurchaseID, entity.ProviderID, entity.Amount, entity.PurchaseDate);

            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderPurchaseRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderPurchaseID);
            if (entity == null) return Messages.ProviderPurchaseMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.WalletID = request.WalletID;
            entity.Amount = request.Amount;
            entity.Remark = request.Remark;
            entity.PurchaseDate = request.PurchaseDate;
            entity.PurchaseDOCInt = request.PurchaseDOCInt;
            entity.IsDelete = request.IsDelete;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderPurchaseMessages.NotFound;
        }
    }
}
