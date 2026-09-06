using FintechApp.Requests.CircleSwitching;
using FintechApp.Responses.CircleSwitching;

namespace FintechApp.Interfaces.Services
{
    public interface ICircleSwitchingService
    {
        Task<List<CircleSwitchingOperatorResponse>> GetOperatorCircleMatrix(int providerId, int serviceId);
        Task<string> UpdateCircles(UpdateCircleSwitchingRequest request);
        Task<List<CircleSwitchingOperatorResponse>> GetAllCircleSwitching();
        Task<string> DeleteByOperatorId(int operatorId);
    }
}
