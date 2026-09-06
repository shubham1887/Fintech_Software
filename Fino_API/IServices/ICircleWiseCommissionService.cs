using FintechApp.Requests.CircleWiseCommission;
using FintechApp.Responses.CircleWiseCommission;

namespace FintechApp.IServices
{
    public interface ICircleWiseCommissionService
    {
        Task<List<CircleWiseCommissionResponse>> GetAll();
        Task<CircleWiseCommissionResponse?> GetByID(int id);
        Task<List<CircleWiseCommissionResponse>> GetBySlabMasterID(int slabMasterID);
        Task<string> Create(CreateCircleWiseCommissionRequest request);
        Task<string> Update(UpdateCircleWiseCommissionRequest request);
        Task<string> Delete(int id);
    }
}
