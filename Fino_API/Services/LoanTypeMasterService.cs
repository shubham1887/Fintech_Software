using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LoanTypeMaster;
using FintechApp.Responses.LoanTypeMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class LoanTypeMasterService : ILoanTypeMasterService
    {
        private readonly ILoanTypeMasterRepository _repository;
        public LoanTypeMasterService(ILoanTypeMasterRepository repository) { _repository = repository; }
        private static LoanTypeMasterResponse MapToResponse(LoanTypeMaster e) => new() { LoanTypeMasterID = e.LoanTypeMasterID, LoanType = e.LoanType, };
        public async Task<List<LoanTypeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LoanTypeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLoanTypeMasterRequest request) { var entity = new LoanTypeMaster { LoanType = request.LoanType, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateLoanTypeMasterRequest request) { var entity = await _repository.GetByID(request.LoanTypeMasterID); if (entity == null) return Messages.LoanTypeMasterMessages.NotFound; entity.LoanType = request.LoanType; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.LoanTypeMasterMessages.NotFound; }
    }
}
