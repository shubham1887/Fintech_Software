using FintechApp.Requests.CircleAPIMaster;
using FintechApp.Responses.CircleAPIMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ICircleAPIMasterService
    {
        Task<List<CircleAPIMasterResponse>> GetAll();
        Task<CircleAPIMasterResponse?> GetByID(int id);
        Task<string> Create(CreateCircleAPIMasterRequest request);
        Task<string> Update(UpdateCircleAPIMasterRequest request);
        Task<string> Delete(int id);
        Task<string> ActiveDeactive(int id, bool isActive);
    }
}
