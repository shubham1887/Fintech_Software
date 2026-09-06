using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserwiseLowBalance;
using FintechApp.Responses.UserwiseLowBalance;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserwiseLowBalanceService : IUserwiseLowBalanceService
    {
        private readonly IUserwiseLowBalanceRepository _repository;

        public UserwiseLowBalanceService(IUserwiseLowBalanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserwiseLowBalanceResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserwiseLowBalanceResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserwiseLowBalanceResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserwiseLowBalanceRequest request)
        {
            var entity = new UserwiseLowBalance
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                Amount = request.Amount
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserwiseLowBalanceRequest request)
        {
            var entity = await _repository.GetByID(request.UserwiseLowBalanceID);
            if (entity == null) return UserwiseLowBalanceMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.Amount = request.Amount;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserwiseLowBalanceMessages.NotFound;
        }

        private static UserwiseLowBalanceResponse MapToResponse(UserwiseLowBalance e) => new()
        {
            UserwiseLowBalanceID = e.UserwiseLowBalanceID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            Amount = e.Amount
        };
    }
}
