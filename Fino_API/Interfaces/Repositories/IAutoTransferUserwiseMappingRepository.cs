using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAutoTransferUserwiseMappingRepository
    {
        Task<List<AutoTransferUserwiseMapping>> GetAll();
        Task<AutoTransferUserwiseMapping?> GetByID(int id);
        Task<List<AutoTransferUserwiseMapping>> GetByUserID(int userID);
        Task<int> Insert(AutoTransferUserwiseMapping entity);
        Task<int> Update(AutoTransferUserwiseMapping entity);
        Task<int> Delete(int id);
    }
}
