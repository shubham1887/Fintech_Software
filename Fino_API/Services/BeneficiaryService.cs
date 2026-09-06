using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repository;
        public BeneficiaryService(IBeneficiaryRepository repository) { _repository = repository; }
        private static BeneficiaryResponse MapToResponse(Beneficiary e) => new() { BeneficiaryID = e.BeneficiaryID, Name = e.Name, BankName = e.BankName, AccountNo = e.AccountNo, IFSC = e.IFSC, GUID = e.GUID, SenderMobile = e.SenderMobile, IPAddress = e.IPAddress, SenderID = e.SenderID, UserID = e.UserID, IsActive = e.IsActive, IsOTPVerified = e.IsOTPVerified, IsNameVerified = e.IsNameVerified, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<BeneficiaryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BeneficiaryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<BeneficiaryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateBeneficiaryRequest request) { var entity = new Beneficiary { Name = request.Name, BankName = request.BankName, AccountNo = request.AccountNo, IFSC = request.IFSC, GUID = request.GUID, SenderMobile = request.SenderMobile, IPAddress = request.IPAddress, SenderID = request.SenderID, UserID = request.UserID, IsActive = request.IsActive, IsOTPVerified = request.IsOTPVerified, IsNameVerified = request.IsNameVerified, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateBeneficiaryRequest request) { var entity = await _repository.GetByID(request.BeneficiaryID); if (entity == null) return Messages.BeneficiaryMessages.NotFound; entity.Name = request.Name; entity.BankName = request.BankName; entity.AccountNo = request.AccountNo; entity.IFSC = request.IFSC; entity.GUID = request.GUID; entity.SenderMobile = request.SenderMobile; entity.IPAddress = request.IPAddress; entity.SenderID = request.SenderID; entity.UserID = request.UserID; entity.IsActive = request.IsActive; entity.IsOTPVerified = request.IsOTPVerified; entity.IsNameVerified = request.IsNameVerified; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.BeneficiaryMessages.NotFound; }
    }
}
