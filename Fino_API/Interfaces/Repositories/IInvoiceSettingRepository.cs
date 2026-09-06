using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IInvoiceSettingRepository
    {
        Task<List<InvoiceSetting>> GetAll();
        Task<InvoiceSetting?> GetByID(int id);
        Task<int> Insert(InvoiceSetting entity);
        Task<int> Update(InvoiceSetting entity);
        Task<int> Delete(int id);
    }
}
