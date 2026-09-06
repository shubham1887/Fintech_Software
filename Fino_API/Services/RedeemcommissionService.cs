using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Redeemcommission;
using FintechApp.Responses.Redeemcommission;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class RedeemcommissionService : IRedeemcommissionService
    {
        private readonly IRedeemcommissionRepository _repository;
        public RedeemcommissionService(IRedeemcommissionRepository repository) { _repository = repository; }

        private static RedeemcommissionResponse MapToResponse(Redeemcommission e) => new()
        {
            RedeemcommissionID = e.RedeemcommissionID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            Month = e.Month,
            Year = e.Year,
            Amount = e.Amount,
            AdminRemark = e.AdminRemark,
            Status = e.Status,
            LastYearMonth = e.LastYearMonth,
        };

        public async Task<List<RedeemcommissionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<RedeemcommissionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<RedeemcommissionResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateRedeemcommissionRequest request)
        {
            var entity = new Redeemcommission
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                Month = request.Month,
                Year = request.Year,
                Amount = request.Amount,
                AdminRemark = request.AdminRemark,
                Status = request.Status,
                LastYearMonth = request.LastYearMonth,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateRedeemcommissionRequest request)
        {
            var entity = await _repository.GetByID(request.RedeemcommissionID);
            if (entity == null) return Messages.RedeemcommissionMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.Month = request.Month;
            entity.Year = request.Year;
            entity.Amount = request.Amount;
            entity.AdminRemark = request.AdminRemark;
            entity.Status = request.Status;
            entity.LastYearMonth = request.LastYearMonth;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.RedeemcommissionMessages.NotFound;
        }
    }
}
