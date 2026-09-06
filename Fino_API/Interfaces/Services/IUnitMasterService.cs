using FintechApp.Requests.UnitMaster;
using FintechApp.Responses.UnitMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUnitMasterService
    {
        Task<List<UnitMasterResponse>> GetAll();
        Task<UnitMasterResponse?> GetByID(int id);

        Task<string> Create(CreateUnitMasterRequest request);
        Task<string> Update(UpdateUnitMasterRequest request);
        Task<string> Delete(int id);
    }
}
