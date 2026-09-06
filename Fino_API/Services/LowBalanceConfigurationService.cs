using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LowBalanceConfiguration;
using FintechApp.Responses.LowBalanceConfiguration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LowBalanceConfigurationService : ILowBalanceConfigurationService
    {
        private readonly ILowBalanceConfigurationRepository _repository;

        public LowBalanceConfigurationService(ILowBalanceConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LowBalanceConfigurationResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<LowBalanceConfigurationResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<LowBalanceConfigurationResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateLowBalanceConfigurationRequest request)
        {
            var entity = new LowBalanceConfiguration
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                Amount = request.Amount,
                MobileNumber = request.MobileNumber,
                EmailID = request.EmailID,
                WebHookURL = request.WebHookURL,
                IsActive = request.IsActive,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateLowBalanceConfigurationRequest request)
        {
            var entity = await _repository.GetByID(request.LowBalanceConfigurationID);
            if (entity == null) return Messages.LowBalanceConfigurationMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.Amount = request.Amount;
            entity.MobileNumber = request.MobileNumber;
            entity.EmailID = request.EmailID;
            entity.WebHookURL = request.WebHookURL;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.Now;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.LowBalanceConfigurationMessages.NotFound;
        }

        private static LowBalanceConfigurationResponse MapToResponse(LowBalanceConfiguration e) => new()
        {
            LowBalanceConfigurationID = e.LowBalanceConfigurationID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            Amount = e.Amount,
            MobileNumber = e.MobileNumber,
            EmailID = e.EmailID,
            WebHookURL = e.WebHookURL,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
