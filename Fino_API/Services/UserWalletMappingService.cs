using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserWalletMapping;
using FintechApp.Responses.UserWalletMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserWalletMappingService : IUserWalletMappingService
    {
        private readonly IUserWalletMappingRepository _repository;

        public UserWalletMappingService(IUserWalletMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserWalletMappingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserWalletMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserWalletMappingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserWalletMappingRequest request)
        {
            var entity = new UserWalletMapping
            {
                UserID = request.UserID,
                WalletID = request.WalletID
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserWalletMappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserWalletMappingID);
            if (entity == null) return UserWalletMappingMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserWalletMappingMessages.NotFound;
        }

        private static UserWalletMappingResponse MapToResponse(UserWalletMapping e) => new()
        {
            UserWalletMappingID = e.UserWalletMappingID,
            UserID = e.UserID,
            WalletID = e.WalletID
        };
    }
}
