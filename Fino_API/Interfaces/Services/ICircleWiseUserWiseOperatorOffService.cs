using FintechApp.Requests.CircleWiseUserWiseOperatorOff;
using FintechApp.Responses.CircleWiseUserWiseOperatorOff;
namespace FintechApp.Interfaces.Services
{
    public interface ICircleWiseUserWiseOperatorOffService
    {
        Task<List<CircleWiseUserWiseOperatorOffResponse>> GetAll();
        Task<CircleWiseUserWiseOperatorOffResponse?> GetByID(int id);
        Task<List<CircleWiseUserWiseOperatorOffResponse>> GetByUserID(int userID);
        Task<string> Create(CreateCircleWiseUserWiseOperatorOffRequest request);
        Task<string> Update(UpdateCircleWiseUserWiseOperatorOffRequest request);
        Task<string> Delete(int id);
    }
}
