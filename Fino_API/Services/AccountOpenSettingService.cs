using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AccountOpenSetting;
using FintechApp.Responses.AccountOpenSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class AccountOpenSettingService : IAccountOpenSettingService
    {
        private readonly IAccountOpenSettingRepository _repository;
        public AccountOpenSettingService(IAccountOpenSettingRepository repository) { _repository = repository; }

        private static AccountOpenSettingResponse MapToResponse(AccountOpenSetting e) => new()
        {
            AccountOpenSettingID = e.AccountOpenSettingID,
            Name = e.Name,
            IsActiveForWebsite = e.IsActiveForWebsite,
            IsActive = e.IsActive,
            UserName = e.UserName,
            MerchantID = e.MerchantID,
            Token = e.Token,
            CallBackURL = e.CallBackURL,
            DisplayName = e.DisplayName,
            Sequence = e.Sequence,
            Mode = e.Mode
        };

        public async Task<List<AccountOpenSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<AccountOpenSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateAccountOpenSettingRequest request)
        {
            var entity = new AccountOpenSetting
            {
                Name = request.Name,
                IsActiveForWebsite = request.IsActiveForWebsite,
                IsActive = request.IsActive,
                UserName = request.UserName,
                MerchantID = request.MerchantID,
                Token = request.Token,
                CallBackURL = request.CallBackURL,
                DisplayName = request.DisplayName,
                Sequence = request.Sequence,
                Mode = request.Mode
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateAccountOpenSettingRequest request)
        {
            var entity = await _repository.GetByID(request.AccountOpenSettingID);
            if (entity == null) return Messages.AccountOpenSettingMessages.NotFound;
            entity.Name = request.Name;
            entity.IsActiveForWebsite = request.IsActiveForWebsite;
            entity.IsActive = request.IsActive;
            entity.UserName = request.UserName;
            entity.MerchantID = request.MerchantID;
            entity.Token = request.Token;
            entity.CallBackURL = request.CallBackURL;
            entity.DisplayName = request.DisplayName;
            entity.Sequence = request.Sequence;
            entity.Mode = request.Mode;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.AccountOpenSettingMessages.NotFound;
        }
    }
}
