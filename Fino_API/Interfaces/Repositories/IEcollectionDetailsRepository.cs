using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IEcollectionDetailsRepository
    {
        Task<List<EcollectionDetails>> GetAll();
        Task<EcollectionDetails?> GetByID(int id);
        Task<List<EcollectionDetails>> GetByUserID(int userID);
        Task<int> Insert(EcollectionDetails entity);
        Task<int> Update(EcollectionDetails entity);
        Task<int> Delete(int id);
    }
}
