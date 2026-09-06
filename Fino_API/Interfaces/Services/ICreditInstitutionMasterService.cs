using FintechApp.Requests.CreditInstitutionMaster;
using FintechApp.Responses.CreditInstitutionMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ICreditInstitutionMasterService
    {
        Task<List<CreditInstitutionMasterResponse>> GetAll();
        Task<CreditInstitutionMasterResponse?> GetByID(int id);

        Task<string> Create(CreateCreditInstitutionMasterRequest request);
        Task<string> Update(UpdateCreditInstitutionMasterRequest request);
        Task<string> Delete(int id);
    }
}
