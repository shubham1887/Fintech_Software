using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ReferralRegistration;
using FintechApp.Responses.ReferralRegistration;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ReferralRegistrationService : IReferralRegistrationService
    {
        private readonly IReferralRegistrationRepository _repository;
        public ReferralRegistrationService(IReferralRegistrationRepository repository) { _repository = repository; }
        private static ReferralRegistrationResponse MapToResponse(ReferralRegistration e) => new() { ReferralRegistrationID = e.ReferralRegistrationID, UserID = e.UserID, MobileNumber = e.MobileNumber, ReferralCode = e.ReferralCode, CreatedByUserID = e.CreatedByUserID, IsBenefitUtilize = e.IsBenefitUtilize, ReferralAmount = e.ReferralAmount, };
        public async Task<List<ReferralRegistrationResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ReferralRegistrationResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ReferralRegistrationResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateReferralRegistrationRequest request) { var entity = new ReferralRegistration { UserID = request.UserID, MobileNumber = request.MobileNumber, ReferralCode = request.ReferralCode, CreatedByUserID = request.CreatedByUserID, IsBenefitUtilize = request.IsBenefitUtilize, ReferralAmount = request.ReferralAmount, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateReferralRegistrationRequest request) { var entity = await _repository.GetByID(request.ReferralRegistrationID); if (entity == null) return Messages.ReferralRegistrationMessages.NotFound; entity.UserID = request.UserID; entity.MobileNumber = request.MobileNumber; entity.ReferralCode = request.ReferralCode; entity.CreatedByUserID = request.CreatedByUserID; entity.IsBenefitUtilize = request.IsBenefitUtilize; entity.ReferralAmount = request.ReferralAmount; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ReferralRegistrationMessages.NotFound; }
    }
}
