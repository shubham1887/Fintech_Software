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
        public ProviderPurchaseService(IProviderPurchaseRepository repository) { _repository = repository; }

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
        };

        public async Task<List<ProviderPurchaseResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderPurchaseResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ProviderPurchaseResponse>> GetByProviderID(int providerid) { var list = await _repository.GetByProviderID(providerid); return list.Select(MapToResponse).ToList(); }
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
