using FintechApp.Requests.SlabMaster;
using FintechApp.Responses.SlabMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ISlabMasterService
    {
        Task<List<SlabMasterResponse>> GetAll();
        Task<SlabMasterResponse?> GetByID(int slabMasterId);
        Task<List<SlabMasterDropdownResponse>> GetDropdown();
        Task<string> Create(CreateSlabMasterRequest request);
        Task<string> Update(UpdateSlabMasterRequest request);
        Task<string> Delete(int slabMasterId);
    }
}
