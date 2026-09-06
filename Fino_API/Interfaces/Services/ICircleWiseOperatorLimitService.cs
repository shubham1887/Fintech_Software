using FintechApp.Requests.CircleWiseOperatorLimit;
using FintechApp.Responses.CircleWiseOperatorLimit;
namespace FintechApp.Interfaces.Services
{
    public interface ICircleWiseOperatorLimitService
    {
        Task<List<CircleWiseOperatorLimitResponse>> GetAll();
        Task<CircleWiseOperatorLimitResponse?> GetByID(int id);
        Task<List<CircleWiseOperatorLimitResponse>> GetByUserID(int userID);
        Task<string> Create(CreateCircleWiseOperatorLimitRequest request);
        Task<string> Update(UpdateCircleWiseOperatorLimitRequest request);
        Task<string> Delete(int id);
    }
}
