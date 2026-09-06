using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.FailedSignUpData;
using FintechApp.Responses.FailedSignUpData;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class FailedSignUpDataService : IFailedSignUpDataService
    {
        private readonly IFailedSignUpDataRepository _repository;
        public FailedSignUpDataService(IFailedSignUpDataRepository repository) { _repository = repository; }
        private static FailedSignUpDataResponse MapToResponse(FailedSignUpData e) => new() { FailedSignUpDataID = e.FailedSignUpDataID, MobileNumber = e.MobileNumber, PANNo = e.PANNo, ReferralCode = e.ReferralCode, EmployeeID = e.EmployeeID, OrderID = e.OrderID, RedirectURL = e.RedirectURL, Status = e.Status, PinCode = e.PinCode, Description = e.Description, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<FailedSignUpDataResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<FailedSignUpDataResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateFailedSignUpDataRequest request) { var entity = new FailedSignUpData { MobileNumber = request.MobileNumber, PANNo = request.PANNo, ReferralCode = request.ReferralCode, EmployeeID = request.EmployeeID, OrderID = request.OrderID, RedirectURL = request.RedirectURL, Status = request.Status, PinCode = request.PinCode, Description = request.Description, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateFailedSignUpDataRequest request) { var entity = await _repository.GetByID(request.FailedSignUpDataID); if (entity == null) return Messages.FailedSignUpDataMessages.NotFound; entity.MobileNumber = request.MobileNumber; entity.PANNo = request.PANNo; entity.ReferralCode = request.ReferralCode; entity.EmployeeID = request.EmployeeID; entity.OrderID = request.OrderID; entity.RedirectURL = request.RedirectURL; entity.Status = request.Status; entity.PinCode = request.PinCode; entity.Description = request.Description; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.FailedSignUpDataMessages.NotFound; }
    }
}
