using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IDateFormatMasterRepository
    {
        Task<List<DateFormatMaster>> GetAll();
        Task<DateFormatMaster?> GetByID(int id);
        Task<int> Insert(DateFormatMaster entity);
        Task<int> Update(DateFormatMaster entity);
        Task<int> Delete(int id);
    }
}
