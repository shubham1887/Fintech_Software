using FintechApp.Requests.UserWisePlanView;
using FintechApp.Responses.UserWisePlanView;
namespace FintechApp.Interfaces.Services
{
    public interface IUserWisePlanViewService
    {
        Task<List<UserWisePlanViewResponse>> GetAll();
        Task<UserWisePlanViewResponse?> GetByID(int id);
        Task<List<UserWisePlanViewResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserWisePlanViewRequest request);
        Task<string> Update(UpdateUserWisePlanViewRequest request);
        Task<string> Delete(int id);
    }
}
