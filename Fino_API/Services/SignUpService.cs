using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SignUp;
using FintechApp.Responses.SignUp;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly ISignUpRepository _repository;
        public SignUpService(ISignUpRepository repository) { _repository = repository; }
        private static SignUpResponse MapToResponse(SignUp e) => new() { SignUpID = e.SignUpID, MobileNumber = e.MobileNumber, PANNo = e.PANNo, ReferralCode = e.ReferralCode, EmployeeID = e.EmployeeID, OrderID = e.OrderID, ProfileID = e.ProfileID, RedirectURL = e.RedirectURL, Status = e.Status, PinCode = e.PinCode, };
        public async Task<List<SignUpResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SignUpResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateSignUpRequest request) { var entity = new SignUp { MobileNumber = request.MobileNumber, PANNo = request.PANNo, ReferralCode = request.ReferralCode, EmployeeID = request.EmployeeID, OrderID = request.OrderID, ProfileID = request.ProfileID, RedirectURL = request.RedirectURL, Status = request.Status, PinCode = request.PinCode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSignUpRequest request) { var entity = await _repository.GetByID(request.SignUpID); if (entity == null) return Messages.SignUpMessages.NotFound; entity.MobileNumber = request.MobileNumber; entity.PANNo = request.PANNo; entity.ReferralCode = request.ReferralCode; entity.EmployeeID = request.EmployeeID; entity.OrderID = request.OrderID; entity.ProfileID = request.ProfileID; entity.RedirectURL = request.RedirectURL; entity.Status = request.Status; entity.PinCode = request.PinCode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SignUpMessages.NotFound; }
    }
}
