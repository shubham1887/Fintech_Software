using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IInvoiceNumberHistoryRepository
    {
        Task<List<InvoiceNumberHistory>> GetAll();
        Task<InvoiceNumberHistory?> GetByID(int id);
        Task<List<InvoiceNumberHistory>> GetByUserID(int userID);
        Task<int> Insert(InvoiceNumberHistory entity);
        Task<int> Update(InvoiceNumberHistory entity);
        Task<int> Delete(int id);
    }
}
