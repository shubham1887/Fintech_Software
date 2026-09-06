using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UpiSender;
using FintechApp.Responses.UpiSender;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UpiSenderService : IUpiSenderService
    {
        private readonly IUpiSenderRepository _repository;
        public UpiSenderService(IUpiSenderRepository repository) { _repository = repository; }
        private static UpiSenderResponse MapToResponse(UpiSender e) => new() { UpiSenderID = e.UpiSenderID, Name = e.Name, Mobile = e.Mobile, GUID = e.GUID, Address = e.Address, UserID = e.UserID, IPAddress = e.IPAddress, Pincode = e.Pincode, };
        public async Task<List<UpiSenderResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UpiSenderResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UpiSenderResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUpiSenderRequest request) { var entity = new UpiSender { Name = request.Name, Mobile = request.Mobile, GUID = request.GUID, Address = request.Address, UserID = request.UserID, IPAddress = request.IPAddress, Pincode = request.Pincode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUpiSenderRequest request) { var entity = await _repository.GetByID(request.UpiSenderID); if (entity == null) return Messages.UpiSenderMessages.NotFound; entity.Name = request.Name; entity.Mobile = request.Mobile; entity.GUID = request.GUID; entity.Address = request.Address; entity.UserID = request.UserID; entity.IPAddress = request.IPAddress; entity.Pincode = request.Pincode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UpiSenderMessages.NotFound; }
    }
}
