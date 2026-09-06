using FintechApp.Requests.TransactionStatusChange;
using FintechApp.Responses.TransactionStatusChange;
namespace FintechApp.Interfaces.Services
{
    public interface ITransactionStatusChangeService
    {
        Task<List<TransactionStatusChangeResponse>> GetAll();
        Task<TransactionStatusChangeResponse?> GetByID(int id);
        Task<List<TransactionStatusChangeResponse>> GetByUserID(int userid);
        Task<string> Create(CreateTransactionStatusChangeRequest request);
        Task<string> Update(UpdateTransactionStatusChangeRequest request);
        Task<string> Delete(int id);
    }
}
