using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserBalanceHistory;
using FintechApp.Responses.UserBalanceHistory;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserBalanceHistoryService : IUserBalanceHistoryService
    {
        private readonly IUserBalanceHistoryRepository _repository;

        public UserBalanceHistoryService(IUserBalanceHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserBalanceHistoryResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserBalanceHistoryResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserBalanceHistoryResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserBalanceHistoryRequest request)
        {
            var entity = new UserBalanceHistory
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                Balance = request.Balance,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserBalanceHistoryRequest request)
        {
            var entity = await _repository.GetByID(request.UserBalanceHistoryID);
            if (entity == null) return UserBalanceHistoryMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.Balance = request.Balance;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserBalanceHistoryMessages.NotFound;
        }

        private static UserBalanceHistoryResponse MapToResponse(UserBalanceHistory e) => new()
        {
            UserBalanceHistoryID = e.UserBalanceHistoryID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            Balance = e.Balance,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
