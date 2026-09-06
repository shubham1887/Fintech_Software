using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserCreationWallet;
using FintechApp.Responses.UserCreationWallet;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserCreationWalletService : IUserCreationWalletService
    {
        private readonly IUserCreationWalletRepository _repository;

        public UserCreationWalletService(IUserCreationWalletRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserCreationWalletResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserCreationWalletResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserCreationWalletResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserCreationWalletRequest request)
        {
            var entity = new UserCreationWallet
            {
                IdCount = request.IdCount,
                UserID = request.UserID
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserCreationWalletRequest request)
        {
            var entity = await _repository.GetByID(request.UserCreationWalletID);
            if (entity == null) return UserCreationWalletMessages.NotFound;
            entity.IdCount = request.IdCount;
            entity.UserID = request.UserID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserCreationWalletMessages.NotFound;
        }

        private static UserCreationWalletResponse MapToResponse(UserCreationWallet e) => new()
        {
            UserCreationWalletID = e.UserCreationWalletID,
            IdCount = e.IdCount,
            UserID = e.UserID
        };
    }
}
