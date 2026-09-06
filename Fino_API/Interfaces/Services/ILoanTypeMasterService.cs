using FintechApp.Requests.LoanTypeMaster;
using FintechApp.Responses.LoanTypeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ILoanTypeMasterService
    {
        Task<List<LoanTypeMasterResponse>> GetAll();
        Task<LoanTypeMasterResponse?> GetByID(int id);

        Task<string> Create(CreateLoanTypeMasterRequest request);
        Task<string> Update(UpdateLoanTypeMasterRequest request);
        Task<string> Delete(int id);
    }
}
