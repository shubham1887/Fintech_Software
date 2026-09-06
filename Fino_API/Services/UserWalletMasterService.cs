using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserWalletMaster;
using FintechApp.Responses.UserWalletMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserWalletMasterService : IUserWalletMasterService
    {
        private readonly IUserWalletMasterRepository _repository;

        public UserWalletMasterService(IUserWalletMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserWalletMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserWalletMasterResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserWalletMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserWalletMasterRequest request)
        {
            var entity = new UserWalletMaster
            {
                WalletID = request.WalletID,
                Balance = request.Balance,
                UserID = request.UserID,
                BalanceHash = request.BalanceHash
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserWalletMasterRequest request)
        {
            var entity = await _repository.GetByID(request.UserWalletMasterID);
            if (entity == null) return UserWalletMasterMessages.NotFound;
            entity.WalletID = request.WalletID;
            entity.Balance = request.Balance;
            entity.UserID = request.UserID;
            entity.BalanceHash = request.BalanceHash;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserWalletMasterMessages.NotFound;
        }

        private static UserWalletMasterResponse MapToResponse(UserWalletMaster e) => new()
        {
            UserWalletMasterID = e.UserWalletMasterID,
            WalletID = e.WalletID,
            Balance = e.Balance,
            UserID = e.UserID,
            BalanceHash = e.BalanceHash
        };
    }
}
