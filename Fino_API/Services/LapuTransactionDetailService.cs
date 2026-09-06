using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LapuTransactionDetail;
using FintechApp.Responses.LapuTransactionDetail;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class LapuTransactionDetailService : ILapuTransactionDetailService
    {
        private readonly ILapuTransactionDetailRepository _repository;
        public LapuTransactionDetailService(ILapuTransactionDetailRepository repository) { _repository = repository; }
        private static LapuTransactionDetailResponse MapToResponse(LapuTransactionDetail e) => new() { LapuTransactionDetailID = e.LapuTransactionDetailID, UserID = e.UserID, OperatorID = e.OperatorID, ProivderID = e.ProivderID, LapuMasterID = e.LapuMasterID, LapuPartyMasterID = e.LapuPartyMasterID, LapuNo = e.LapuNo, Status = e.Status, ConsumerNumber = e.ConsumerNumber, Amount = e.Amount, OpeningBalance = e.OpeningBalance, ClosingBalance = e.ClosingBalance, };
        public async Task<List<LapuTransactionDetailResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LapuTransactionDetailResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<LapuTransactionDetailResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateLapuTransactionDetailRequest request) { var entity = new LapuTransactionDetail { UserID = request.UserID, OperatorID = request.OperatorID, ProivderID = request.ProivderID, LapuMasterID = request.LapuMasterID, LapuPartyMasterID = request.LapuPartyMasterID, LapuNo = request.LapuNo, Status = request.Status, ConsumerNumber = request.ConsumerNumber, Amount = request.Amount, OpeningBalance = request.OpeningBalance, ClosingBalance = request.ClosingBalance, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateLapuTransactionDetailRequest request) { var entity = await _repository.GetByID(request.LapuTransactionDetailID); if (entity == null) return Messages.LapuTransactionDetailMessages.NotFound; entity.UserID = request.UserID; entity.OperatorID = request.OperatorID; entity.ProivderID = request.ProivderID; entity.LapuMasterID = request.LapuMasterID; entity.LapuPartyMasterID = request.LapuPartyMasterID; entity.LapuNo = request.LapuNo; entity.Status = request.Status; entity.ConsumerNumber = request.ConsumerNumber; entity.Amount = request.Amount; entity.OpeningBalance = request.OpeningBalance; entity.ClosingBalance = request.ClosingBalance; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.LapuTransactionDetailMessages.NotFound; }
    }
}
