using FintechApp.Requests.UpiBeneficiary;
using FintechApp.Responses.UpiBeneficiary;
namespace FintechApp.Interfaces.Services
{
    public interface IUpiBeneficiaryService
    {
        Task<List<UpiBeneficiaryResponse>> GetAll();
        Task<UpiBeneficiaryResponse?> GetByID(int id);
        Task<List<UpiBeneficiaryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUpiBeneficiaryRequest request);
        Task<string> Update(UpdateUpiBeneficiaryRequest request);
        Task<string> Delete(int id);
    }
}
