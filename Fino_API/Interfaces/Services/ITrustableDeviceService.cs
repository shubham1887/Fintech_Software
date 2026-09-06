using FintechApp.Requests.TrustableDevice;
using FintechApp.Responses.TrustableDevice;
namespace FintechApp.Interfaces.Services
{
    public interface ITrustableDeviceService
    {
        Task<List<TrustableDeviceResponse>> GetAll();
        Task<TrustableDeviceResponse?> GetByID(int id);
        Task<List<TrustableDeviceResponse>> GetByUserID(int userid);
        Task<string> Create(CreateTrustableDeviceRequest request);
        Task<string> Update(UpdateTrustableDeviceRequest request);
        Task<string> Delete(int id);
    }
}
