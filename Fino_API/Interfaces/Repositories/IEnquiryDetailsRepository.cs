using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IEnquiryDetailsRepository
    {
        Task<List<EnquiryDetails>> GetAll();
        Task<EnquiryDetails?> GetByID(int id);
        Task<int> Insert(EnquiryDetails entity);
        Task<int> Update(EnquiryDetails entity);
        Task<int> Delete(int id);
    }
}
