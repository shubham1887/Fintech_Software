using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OverdraftDetail;
using FintechApp.Responses.OverdraftDetail;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OverdraftDetailService : IOverdraftDetailService
    {
        private readonly IOverdraftDetailRepository _repository;
        public OverdraftDetailService(IOverdraftDetailRepository repository) { _repository = repository; }
        private static OverdraftDetailResponse MapToResponse(OverdraftDetail e) => new() { OverdraftDetailID = e.OverdraftDetailID, UserID = e.UserID, WalletID = e.WalletID, OverdraftOpening = e.OverdraftOpening, OverdraftAmount = e.OverdraftAmount, OverdraftClosing = e.OverdraftClosing, UserRemark = e.UserRemark, AdminRemark = e.AdminRemark, Status = e.Status, OpeningWalletBalance = e.OpeningWalletBalance, };
        public async Task<List<OverdraftDetailResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OverdraftDetailResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OverdraftDetailResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateOverdraftDetailRequest request) { var entity = new OverdraftDetail { UserID = request.UserID, WalletID = request.WalletID, OverdraftOpening = request.OverdraftOpening, OverdraftAmount = request.OverdraftAmount, OverdraftClosing = request.OverdraftClosing, UserRemark = request.UserRemark, AdminRemark = request.AdminRemark, Status = request.Status, OpeningWalletBalance = request.OpeningWalletBalance, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOverdraftDetailRequest request) { var entity = await _repository.GetByID(request.OverdraftDetailID); if (entity == null) return Messages.OverdraftDetailMessages.NotFound; entity.UserID = request.UserID; entity.WalletID = request.WalletID; entity.OverdraftOpening = request.OverdraftOpening; entity.OverdraftAmount = request.OverdraftAmount; entity.OverdraftClosing = request.OverdraftClosing; entity.UserRemark = request.UserRemark; entity.AdminRemark = request.AdminRemark; entity.Status = request.Status; entity.OpeningWalletBalance = request.OpeningWalletBalance; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OverdraftDetailMessages.NotFound; }
    }
}
