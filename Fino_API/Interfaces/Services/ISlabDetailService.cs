using FintechApp.Requests.SlabDetail;
using FintechApp.Responses.SlabDetail;

namespace FintechApp.Interfaces.Services
{
    public interface ISlabDetailService
    {
        Task<List<SlabDetailResponse>> GetAll();
        Task<List<SlabDetailResponse>> GetBySlabMasterID(int slabMasterId);
        Task<SlabDetailResponse?> GetByID(int slabDetailId);
        // byUserID / ipAddress are for the delegation cascade: a change to a band on this slab
        // re-checks every card cut from it, and the invalidation that may follow is an auditable
        // act attributable to whoever made the edit. Optional so the signature stays usable from
        // anywhere that has no session (background sync, tests).
        Task<string> Create(CreateSlabDetailRequest request, int byUserID = 0, string ipAddress = "");
        Task<string> Update(UpdateSlabDetailRequest request, int byUserID = 0, string ipAddress = "");
        Task<string> Delete(int slabDetailId, int byUserID = 0, string ipAddress = "");
    }
}
