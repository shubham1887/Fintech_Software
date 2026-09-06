using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IVerificationDocumentConfigRepository
    {
        Task<List<VerificationDocumentConfig>> GetAll();
        Task<VerificationDocumentConfig?> GetByID(int id);
        Task<int> Insert(VerificationDocumentConfig entity);
        Task<int> Update(VerificationDocumentConfig entity);
        Task<int> Delete(int id);
    }
}
