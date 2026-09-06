using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UpiBeneficiary;
using FintechApp.Responses.UpiBeneficiary;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UpiBeneficiaryService : IUpiBeneficiaryService
    {
        private readonly IUpiBeneficiaryRepository _repository;
        public UpiBeneficiaryService(IUpiBeneficiaryRepository repository) { _repository = repository; }
        private static UpiBeneficiaryResponse MapToResponse(UpiBeneficiary e) => new() { UpiBeneficiaryID = e.UpiBeneficiaryID, Name = e.Name, UpiID = e.UpiID, GUID = e.GUID, SenderMobile = e.SenderMobile, IPAddress = e.IPAddress, UpiSenderID = e.UpiSenderID, UserID = e.UserID, };
        public async Task<List<UpiBeneficiaryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UpiBeneficiaryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UpiBeneficiaryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUpiBeneficiaryRequest request) { var entity = new UpiBeneficiary { Name = request.Name, UpiID = request.UpiID, GUID = request.GUID, SenderMobile = request.SenderMobile, IPAddress = request.IPAddress, UpiSenderID = request.UpiSenderID, UserID = request.UserID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUpiBeneficiaryRequest request) { var entity = await _repository.GetByID(request.UpiBeneficiaryID); if (entity == null) return Messages.UpiBeneficiaryMessages.NotFound; entity.Name = request.Name; entity.UpiID = request.UpiID; entity.GUID = request.GUID; entity.SenderMobile = request.SenderMobile; entity.IPAddress = request.IPAddress; entity.UpiSenderID = request.UpiSenderID; entity.UserID = request.UserID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UpiBeneficiaryMessages.NotFound; }
    }
}
