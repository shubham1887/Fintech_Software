using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserwiseSaleTarget;
using FintechApp.Responses.UserwiseSaleTarget;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserwiseSaleTargetService : IUserwiseSaleTargetService
    {
        private readonly IUserwiseSaleTargetRepository _repository;
        public UserwiseSaleTargetService(IUserwiseSaleTargetRepository repository) { _repository = repository; }
        private static UserwiseSaleTargetResponse MapToResponse(UserwiseSaleTarget e) => new() { UserwiseSaleTargetID = e.UserwiseSaleTargetID, UserID = e.UserID, WalletID = e.WalletID, ParentID = e.ParentID, TargetAmount = e.TargetAmount, Commission = e.Commission, CommissionType = e.CommissionType, IsDelete = e.IsDelete, };
        public async Task<List<UserwiseSaleTargetResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserwiseSaleTargetResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserwiseSaleTargetResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserwiseSaleTargetRequest request) { var entity = new UserwiseSaleTarget { UserID = request.UserID, WalletID = request.WalletID, ParentID = request.ParentID, TargetAmount = request.TargetAmount, Commission = request.Commission, CommissionType = request.CommissionType, IsDelete = request.IsDelete, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserwiseSaleTargetRequest request) { var entity = await _repository.GetByID(request.UserwiseSaleTargetID); if (entity == null) return Messages.UserwiseSaleTargetMessages.NotFound; entity.UserID = request.UserID; entity.WalletID = request.WalletID; entity.ParentID = request.ParentID; entity.TargetAmount = request.TargetAmount; entity.Commission = request.Commission; entity.CommissionType = request.CommissionType; entity.IsDelete = request.IsDelete; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserwiseSaleTargetMessages.NotFound; }
    }
}
