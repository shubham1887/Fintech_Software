using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserCreationMaster;
using FintechApp.Responses.UserCreationMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserCreationMasterService : IUserCreationMasterService
    {
        private readonly IUserCreationMasterRepository _repository;

        public UserCreationMasterService(IUserCreationMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserCreationMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserCreationMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<UserCreationMasterResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateUserCreationMasterRequest request)
        {
            var entity = new UserCreationMaster
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                WalletID = request.WalletID,
                OpeningIdCount = request.OpeningIdCount,
                PurchaseCount = request.PurchaseCount,
                ClosingIdCount = request.ClosingIdCount,
                Amount = request.Amount,
                OpeningBalance = request.OpeningBalance,
                ClosingBalance = request.ClosingBalance,
                Type = request.Type,
                CreatedUserID = request.CreatedUserID,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserCreationMasterRequest request)
        {
            var entity = await _repository.GetByID(request.UserCreationMasterID);
            if (entity == null) return Messages.UserCreationMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.WalletID = request.WalletID;
            entity.OpeningIdCount = request.OpeningIdCount;
            entity.PurchaseCount = request.PurchaseCount;
            entity.ClosingIdCount = request.ClosingIdCount;
            entity.Amount = request.Amount;
            entity.OpeningBalance = request.OpeningBalance;
            entity.ClosingBalance = request.ClosingBalance;
            entity.Type = request.Type;
            entity.CreatedUserID = request.CreatedUserID;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.UserCreationMasterMessages.NotFound;
        }

        private static UserCreationMasterResponse MapToResponse(UserCreationMaster e) => new()
        {
            UserCreationMasterID = e.UserCreationMasterID,
            UserID = e.UserID,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name,
            WalletID = e.WalletID,
            OpeningIdCount = e.OpeningIdCount,
            PurchaseCount = e.PurchaseCount,
            ClosingIdCount = e.ClosingIdCount,
            Amount = e.Amount,
            OpeningBalance = e.OpeningBalance,
            ClosingBalance = e.ClosingBalance,
            Type = e.Type,
            CreatedUserID = e.CreatedUserID,
            DateTime = e.DateTime
        };
    }
}
