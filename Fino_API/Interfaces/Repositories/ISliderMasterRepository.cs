using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISliderMasterRepository
    {
        Task<List<SliderMaster>> GetAll();
        Task<SliderMaster?> GetByID(int sliderMasterId);
        Task<int> Insert(SliderMaster entity);
        Task<int> Update(SliderMaster entity);
        Task<int> Delete(int sliderMasterId);
    }
}
