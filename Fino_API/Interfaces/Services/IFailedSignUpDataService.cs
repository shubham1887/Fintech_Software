using FintechApp.Requests.FailedSignUpData;
using FintechApp.Responses.FailedSignUpData;
namespace FintechApp.Interfaces.Services
{
    public interface IFailedSignUpDataService
    {
        Task<List<FailedSignUpDataResponse>> GetAll();
        Task<FailedSignUpDataResponse?> GetByID(int id);

        Task<string> Create(CreateFailedSignUpDataRequest request);
        Task<string> Update(UpdateFailedSignUpDataRequest request);
        Task<string> Delete(int id);
    }
}
