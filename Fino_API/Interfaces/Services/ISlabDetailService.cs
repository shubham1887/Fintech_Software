using FintechApp.Requests.SlabDetail;
using FintechApp.Responses.SlabDetail;

namespace FintechApp.Interfaces.Services
{
    public interface ISlabDetailService
    {
        Task<List<SlabDetailResponse>> GetAll();
        Task<List<SlabDetailResponse>> GetBySlabMasterID(int slabMasterId);
        Task<SlabDetailResponse?> GetByID(int slabDetailId);
        Task<string> Create(CreateSlabDetailRequest request);
        Task<string> Update(UpdateSlabDetailRequest request);
        Task<string> Delete(int slabDetailId);
    }
}
