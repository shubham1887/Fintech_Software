using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CompanyMaster;
using FintechApp.Responses.CompanyMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CompanyMasterService : ICompanyMasterService
    {
        private readonly ICompanyMasterRepository _repository;

        public CompanyMasterService(ICompanyMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CompanyMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<CompanyMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateCompanyMasterRequest request)
        {
            var entity = new CompanyMaster { Name = request.Name, Value = request.Value };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCompanyMasterRequest request)
        {
            var entity = await _repository.GetByID(request.CompanyMasterID);
            if (entity == null) return Messages.CompanyMasterMessages.NotFound;
            entity.Name = request.Name;
            entity.Value = request.Value;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CompanyMasterMessages.NotFound;
        }

        private static CompanyMasterResponse MapToResponse(CompanyMaster e) => new()
        {
            CompanyMasterID = e.CompanyMasterID,
            Name = e.Name,
            Value = e.Value
        };
    }
}
