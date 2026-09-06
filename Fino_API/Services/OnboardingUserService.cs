using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OnboardingUser;
using FintechApp.Responses.OnboardingUser;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OnboardingUserService : IOnboardingUserService
    {
        private readonly IOnboardingUserRepository _repository;
        public OnboardingUserService(IOnboardingUserRepository repository) { _repository = repository; }
        private static OnboardingUserResponse MapToResponse(OnboardingUser e) => new() { OnboardingUserID = e.OnboardingUserID, ExternalID = e.ExternalID, ProfileID = e.ProfileID, ReferenceID = e.ReferenceID, Name = e.Name, DOB = e.DOB, PanNo = e.PanNo, AadhaarNo = e.AadhaarNo, MobileNumber = e.MobileNumber, EmailID = e.EmailID, State = e.State, PinCode = e.PinCode, Status = e.Status, AdminStatus = e.AdminStatus, SupportStatus = e.SupportStatus, AdminRemark = e.AdminRemark, SupportRemark = e.SupportRemark, OrderID = e.OrderID, AccountNumber = e.AccountNumber, IFSC = e.IFSC, BankName = e.BankName, SlabID = e.SlabID, AgentID = e.AgentID, };
        public async Task<List<OnboardingUserResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OnboardingUserResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateOnboardingUserRequest request) { var entity = new OnboardingUser { ExternalID = request.ExternalID, ProfileID = request.ProfileID, ReferenceID = request.ReferenceID, Name = request.Name, DOB = request.DOB, PanNo = request.PanNo, AadhaarNo = request.AadhaarNo, MobileNumber = request.MobileNumber, EmailID = request.EmailID, State = request.State, PinCode = request.PinCode, Status = request.Status, AdminStatus = request.AdminStatus, SupportStatus = request.SupportStatus, AdminRemark = request.AdminRemark, SupportRemark = request.SupportRemark, OrderID = request.OrderID, AccountNumber = request.AccountNumber, IFSC = request.IFSC, BankName = request.BankName, SlabID = request.SlabID, AgentID = request.AgentID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOnboardingUserRequest request) { var entity = await _repository.GetByID(request.OnboardingUserID); if (entity == null) return Messages.OnboardingUserMessages.NotFound; entity.ExternalID = request.ExternalID; entity.ProfileID = request.ProfileID; entity.ReferenceID = request.ReferenceID; entity.Name = request.Name; entity.DOB = request.DOB; entity.PanNo = request.PanNo; entity.AadhaarNo = request.AadhaarNo; entity.MobileNumber = request.MobileNumber; entity.EmailID = request.EmailID; entity.State = request.State; entity.PinCode = request.PinCode; entity.Status = request.Status; entity.AdminStatus = request.AdminStatus; entity.SupportStatus = request.SupportStatus; entity.AdminRemark = request.AdminRemark; entity.SupportRemark = request.SupportRemark; entity.OrderID = request.OrderID; entity.AccountNumber = request.AccountNumber; entity.IFSC = request.IFSC; entity.BankName = request.BankName; entity.SlabID = request.SlabID; entity.AgentID = request.AgentID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OnboardingUserMessages.NotFound; }
    }
}
