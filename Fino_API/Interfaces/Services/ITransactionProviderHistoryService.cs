using FintechApp.Requests.TransactionProviderHistory;
using FintechApp.Responses.TransactionProviderHistory;
namespace FintechApp.Interfaces.Services
{
    public interface ITransactionProviderHistoryService
    {
        Task<List<TransactionProviderHistoryResponse>> GetAll();
        Task<TransactionProviderHistoryResponse?> GetByID(int id);

        Task<string> Create(CreateTransactionProviderHistoryRequest request);
        Task<string> Update(UpdateTransactionProviderHistoryRequest request);
        Task<string> Delete(int id);
    }
}
