using FintechApp.Requests.TransactionManualUpdate;
using FintechApp.Responses.TransactionManualUpdate;
namespace FintechApp.Interfaces.Services
{
    public interface ITransactionManualUpdateService
    {
        Task<List<TransactionManualUpdateResponse>> GetAll();
        Task<TransactionManualUpdateResponse?> GetByID(int id);

        Task<string> Create(CreateTransactionManualUpdateRequest request);
        Task<string> Update(UpdateTransactionManualUpdateRequest request);
        Task<string> Delete(int id);
    }
}
