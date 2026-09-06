using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserBalanceCapping;
using FintechApp.Responses.UserBalanceCapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserBalanceCappingService : IUserBalanceCappingService
    {
        private readonly IUserBalanceCappingRepository _repository;

        public UserBalanceCappingService(IUserBalanceCappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserBalanceCappingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserBalanceCappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserBalanceCappingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserBalanceCappingRequest request)
        {
            var entity = new UserBalanceCapping
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                Amount = request.Amount,
                IsDelete = request.IsDelete,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserBalanceCappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserBalanceCappingID);
            if (entity == null) return UserBalanceCappingMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.Amount = request.Amount;
            entity.IsDelete = request.IsDelete;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserBalanceCappingMessages.NotFound;
        }

        private static UserBalanceCappingResponse MapToResponse(UserBalanceCapping e) => new()
        {
            UserBalanceCappingID = e.UserBalanceCappingID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            Amount = e.Amount,
            IsDelete = e.IsDelete,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
