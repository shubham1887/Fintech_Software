using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Sender;
using FintechApp.Responses.Sender;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SenderService : ISenderService
    {
        private readonly ISenderRepository _repository;
        public SenderService(ISenderRepository repository) { _repository = repository; }
        private static SenderResponse MapToResponse(Sender e) => new() { SenderID = e.SenderID, Name = e.Name, Mobile = e.Mobile, GUID = e.GUID, Address = e.Address, UserID = e.UserID, IsActive = e.IsActive, IsOTPVerified = e.IsOTPVerified, IsKYC = e.IsKYC, KYCDocumentURL = e.KYCDocumentURL, IPAddress = e.IPAddress, Pincode = e.Pincode, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<SenderResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SenderResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SenderResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSenderRequest request) { var entity = new Sender { Name = request.Name, Mobile = request.Mobile, GUID = request.GUID, Address = request.Address, UserID = request.UserID, IsActive = request.IsActive, IsOTPVerified = request.IsOTPVerified, IsKYC = request.IsKYC, KYCDocumentURL = request.KYCDocumentURL, IPAddress = request.IPAddress, Pincode = request.Pincode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSenderRequest request) { var entity = await _repository.GetByID(request.SenderID); if (entity == null) return Messages.SenderMessages.NotFound; entity.Name = request.Name; entity.Mobile = request.Mobile; entity.GUID = request.GUID; entity.Address = request.Address; entity.UserID = request.UserID; entity.IsActive = request.IsActive; entity.IsOTPVerified = request.IsOTPVerified; entity.IsKYC = request.IsKYC; entity.KYCDocumentURL = request.KYCDocumentURL; entity.IPAddress = request.IPAddress; entity.Pincode = request.Pincode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SenderMessages.NotFound; }
    }
}
