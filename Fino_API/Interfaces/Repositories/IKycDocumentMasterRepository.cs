using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IKycDocumentMasterRepository
    {
        Task<List<KycDocumentMaster>> GetAll();
        Task<KycDocumentMaster?> GetByID(int id);
        Task<int> Insert(KycDocumentMaster entity);
        Task<int> Update(KycDocumentMaster entity);
        Task<int> Delete(int id);
    }
}
