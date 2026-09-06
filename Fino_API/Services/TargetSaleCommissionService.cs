using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TargetSaleCommission;
using FintechApp.Responses.TargetSaleCommission;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TargetSaleCommissionService : ITargetSaleCommissionService
    {
        private readonly ITargetSaleCommissionRepository _repository;
        public TargetSaleCommissionService(ITargetSaleCommissionRepository repository) { _repository = repository; }
        private static TargetSaleCommissionResponse MapToResponse(TargetSaleCommission e) => new() { TargetSaleCommissionID = e.TargetSaleCommissionID, UserID = e.UserID, WalletID = e.WalletID, ParentID = e.ParentID, SaleAmount = e.SaleAmount, CommissionAmount = e.CommissionAmount, Description = e.Description, SaleMonthDOC = e.SaleMonthDOC, Status = e.Status, };
        public async Task<List<TargetSaleCommissionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TargetSaleCommissionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<TargetSaleCommissionResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateTargetSaleCommissionRequest request) { var entity = new TargetSaleCommission { UserID = request.UserID, WalletID = request.WalletID, ParentID = request.ParentID, SaleAmount = request.SaleAmount, CommissionAmount = request.CommissionAmount, Description = request.Description, SaleMonthDOC = request.SaleMonthDOC, Status = request.Status, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTargetSaleCommissionRequest request) { var entity = await _repository.GetByID(request.TargetSaleCommissionID); if (entity == null) return Messages.TargetSaleCommissionMessages.NotFound; entity.UserID = request.UserID; entity.WalletID = request.WalletID; entity.ParentID = request.ParentID; entity.SaleAmount = request.SaleAmount; entity.CommissionAmount = request.CommissionAmount; entity.Description = request.Description; entity.SaleMonthDOC = request.SaleMonthDOC; entity.Status = request.Status; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TargetSaleCommissionMessages.NotFound; }
    }
}
