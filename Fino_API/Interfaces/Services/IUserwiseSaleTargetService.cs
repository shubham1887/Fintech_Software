using FintechApp.Requests.UserwiseSaleTarget;
using FintechApp.Responses.UserwiseSaleTarget;
namespace FintechApp.Interfaces.Services
{
    public interface IUserwiseSaleTargetService
    {
        Task<List<UserwiseSaleTargetResponse>> GetAll();
        Task<UserwiseSaleTargetResponse?> GetByID(int id);
        Task<List<UserwiseSaleTargetResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserwiseSaleTargetRequest request);
        Task<string> Update(UpdateUserwiseSaleTargetRequest request);
        Task<string> Delete(int id);
    }
}
