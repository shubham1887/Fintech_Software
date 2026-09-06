using FintechApp.Requests.CompanyMaster;
using FintechApp.Responses.CompanyMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ICompanyMasterService
    {
        Task<List<CompanyMasterResponse>> GetAll();
        Task<CompanyMasterResponse?> GetByID(int id);
        Task<string> Create(CreateCompanyMasterRequest request);
        Task<string> Update(UpdateCompanyMasterRequest request);
        Task<string> Delete(int id);
    }
}
