using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LeadGenerationMaster;
using FintechApp.Responses.LeadGenerationMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class LeadGenerationMasterService : ILeadGenerationMasterService
    {
        private readonly ILeadGenerationMasterRepository _repository;
        public LeadGenerationMasterService(ILeadGenerationMasterRepository repository) { _repository = repository; }
        private static LeadGenerationMasterResponse MapToResponse(LeadGenerationMaster e) => new() { LeadGenerationMasterID = e.LeadGenerationMasterID, UserID = e.UserID, FormID = e.FormID, MerchantName = e.MerchantName, ConsumerName = e.ConsumerName, MobileNumber = e.MobileNumber, EmailID = e.EmailID, PanNumber = e.PanNumber, Status = e.Status, LoanType = e.LoanType, Gender = e.Gender, APIUserRequestID = e.APIUserRequestID, DOB = e.DOB, };
        public async Task<List<LeadGenerationMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LeadGenerationMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLeadGenerationMasterRequest request) { var entity = new LeadGenerationMaster { UserID = request.UserID, FormID = request.FormID, MerchantName = request.MerchantName, ConsumerName = request.ConsumerName, MobileNumber = request.MobileNumber, EmailID = request.EmailID, PanNumber = request.PanNumber, Status = request.Status, LoanType = request.LoanType, Gender = request.Gender, APIUserRequestID = request.APIUserRequestID, DOB = request.DOB, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateLeadGenerationMasterRequest request) { var entity = await _repository.GetByID(request.LeadGenerationMasterID); if (entity == null) return Messages.LeadGenerationMasterMessages.NotFound; entity.UserID = request.UserID; entity.FormID = request.FormID; entity.MerchantName = request.MerchantName; entity.ConsumerName = request.ConsumerName; entity.MobileNumber = request.MobileNumber; entity.EmailID = request.EmailID; entity.PanNumber = request.PanNumber; entity.Status = request.Status; entity.LoanType = request.LoanType; entity.Gender = request.Gender; entity.APIUserRequestID = request.APIUserRequestID; entity.DOB = request.DOB; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.LeadGenerationMasterMessages.NotFound; }
    }
}
