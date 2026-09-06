using FintechApp.Requests.SliderMaster;
using FintechApp.Responses.SliderMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ISliderMasterService
    {
        Task<List<SliderMasterResponse>> GetAll();
        Task<SliderMasterResponse?> GetByID(int sliderMasterId);
        Task<string> Create(CreateSliderMasterRequest request);
        Task<string> Update(UpdateSliderMasterRequest request);
        Task<string> Delete(int sliderMasterId);
    }
}
