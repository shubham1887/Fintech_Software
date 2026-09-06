using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PayoutSetting;
using FintechApp.Responses.PayoutSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PayoutSettingService : IPayoutSettingService
    {
        private readonly IPayoutSettingRepository _repository;

        public PayoutSettingService(IPayoutSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PayoutSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<PayoutSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreatePayoutSettingRequest request)
        {
            var entity = new PayoutSetting
            {
                Name = request.Name,
                MerchantID = request.MerchantID,
                Token = request.Token,
                Slat = request.Slat,
                ActionURL = request.ActionURL,
                StatusCheckURL = request.StatusCheckURL,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                Mode = request.Mode
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePayoutSettingRequest request)
        {
            var entity = await _repository.GetByID(request.PayoutSettingID);
            if (entity == null) return Messages.PayoutSettingMessages.NotFound;
            entity.Name = request.Name;
            entity.MerchantID = request.MerchantID;
            entity.Token = request.Token;
            entity.Slat = request.Slat;
            entity.ActionURL = request.ActionURL;
            entity.StatusCheckURL = request.StatusCheckURL;
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
            return result > 0 ? StatusType.Ok : Messages.PayoutSettingMessages.NotFound;
        }

        private static PayoutSettingResponse MapToResponse(PayoutSetting e) => new()
        {
            PayoutSettingID = e.PayoutSettingID,
            Name = e.Name,
            MerchantID = e.MerchantID,
            Token = e.Token,
            Slat = e.Slat,
            ActionURL = e.ActionURL,
            StatusCheckURL = e.StatusCheckURL,
            IsActiveWeb = e.IsActiveWeb,
            IsActive = e.IsActive,
            MinimumAmount = e.MinimumAmount,
            MaximumAmount = e.MaximumAmount,
            Mode = e.Mode
        };
    }
}
