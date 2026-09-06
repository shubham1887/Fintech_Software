using FintechApp.Requests.FingerPrintDeviceType;
using FintechApp.Responses.FingerPrintDeviceType;
namespace FintechApp.Interfaces.Services
{
    public interface IFingerPrintDeviceTypeService
    {
        Task<List<FingerPrintDeviceTypeResponse>> GetAll();
        Task<FingerPrintDeviceTypeResponse?> GetByID(int id);
        Task<string> Create(CreateFingerPrintDeviceTypeRequest request);
        Task<string> Update(UpdateFingerPrintDeviceTypeRequest request);
        Task<string> Delete(int id);
    }
}
