using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserDocumentMasterRepository
    {
        Task<List<UserDocumentMaster>> GetAll();
        Task<UserDocumentMaster?> GetByID(int id);
        Task<List<UserDocumentMaster>> GetByUserID(int userID);
        Task<int> Insert(UserDocumentMaster entity);
        Task<int> Update(UserDocumentMaster entity);
        Task<int> Delete(int id);
    }
}
