using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PureDMTTransferBeneDetail;
using FintechApp.Responses.PureDMTTransferBeneDetail;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class PureDMTTransferBeneDetailService : IPureDMTTransferBeneDetailService
    {
        private readonly IPureDMTTransferBeneDetailRepository _repository;
        public PureDMTTransferBeneDetailService(IPureDMTTransferBeneDetailRepository repository) { _repository = repository; }
        private static PureDMTTransferBeneDetailResponse MapToResponse(PureDMTTransferBeneDetail e) => new() { PureDMTTransferBeneDetailID = e.PureDMTTransferBeneDetailID, SenderNumber = e.SenderNumber, ProviderID = e.ProviderID, BeneID = e.BeneID, AccountNumber = e.AccountNumber, VerifyBeneName = e.VerifyBeneName, IsVerify = e.IsVerify, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<PureDMTTransferBeneDetailResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PureDMTTransferBeneDetailResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePureDMTTransferBeneDetailRequest request) { var entity = new PureDMTTransferBeneDetail { SenderNumber = request.SenderNumber, ProviderID = request.ProviderID, BeneID = request.BeneID, AccountNumber = request.AccountNumber, VerifyBeneName = request.VerifyBeneName, IsVerify = request.IsVerify, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdatePureDMTTransferBeneDetailRequest request) { var entity = await _repository.GetByID(request.PureDMTTransferBeneDetailID); if (entity == null) return Messages.PureDMTTransferBeneDetailMessages.NotFound; entity.SenderNumber = request.SenderNumber; entity.ProviderID = request.ProviderID; entity.BeneID = request.BeneID; entity.AccountNumber = request.AccountNumber; entity.VerifyBeneName = request.VerifyBeneName; entity.IsVerify = request.IsVerify; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.PureDMTTransferBeneDetailMessages.NotFound; }
    }
}
