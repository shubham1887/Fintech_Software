using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CashDepositSetting;
using FintechApp.Responses.CashDepositSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CashDepositSettingService : ICashDepositSettingService
    {
        private readonly ICashDepositSettingRepository _repository;
        public CashDepositSettingService(ICashDepositSettingRepository repository) { _repository = repository; }

        private static CashDepositSettingResponse MapToResponse(CashDepositSetting e) => new()
        {
            CashDepositSettingID = e.CashDepositSettingID,
            Name = e.Name,
            MerchantID = e.MerchantID,
            Token = e.Token,
            Slat = e.Slat,
            PublicKey = e.PublicKey,
            IsActiveWeb = e.IsActiveWeb,
            IsActive = e.IsActive,
            MinimumAmount = e.MinimumAmount,
            MaximumAmount = e.MaximumAmount,
            Mode = e.Mode
        };

        public async Task<List<CashDepositSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CashDepositSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCashDepositSettingRequest request)
        {
            var entity = new CashDepositSetting
            {
                Name = request.Name,
                MerchantID = request.MerchantID,
                Token = request.Token,
                Slat = request.Slat,
                PublicKey = request.PublicKey,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                Mode = request.Mode
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCashDepositSettingRequest request)
        {
            var entity = await _repository.GetByID(request.CashDepositSettingID);
            if (entity == null) return Messages.CashDepositSettingMessages.NotFound;
            entity.Name = request.Name;
            entity.MerchantID = request.MerchantID;
            entity.Token = request.Token;
            entity.Slat = request.Slat;
            entity.PublicKey = request.PublicKey;
            entity.IsActiveWeb = request.IsActiveWeb;
            entity.IsActive = request.IsActive;
            entity.MinimumAmount = request.MinimumAmount;
            entity.MaximumAmount = request.MaximumAmount;
            entity.Mode = request.Mode;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CashDepositSettingMessages.NotFound;
        }
    }
}
