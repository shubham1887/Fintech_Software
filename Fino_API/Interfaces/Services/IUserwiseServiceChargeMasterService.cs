using FintechApp.Requests.UserwiseServiceChargeMaster;
using FintechApp.Responses.UserwiseServiceChargeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUserwiseServiceChargeMasterService
    {
        Task<List<UserwiseServiceChargeMasterResponse>> GetAll();
        Task<UserwiseServiceChargeMasterResponse?> GetByID(int id);
        Task<List<UserwiseServiceChargeMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserwiseServiceChargeMasterRequest request);
        Task<string> Update(UpdateUserwiseServiceChargeMasterRequest request);
        Task<string> Delete(int id);
    }
}
