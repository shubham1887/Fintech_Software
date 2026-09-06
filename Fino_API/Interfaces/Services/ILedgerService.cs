using FintechApp.Requests.Ledger;
using FintechApp.Responses.Ledger;

namespace FintechApp.Interfaces.Services
{
    public interface ILedgerService
    {
        Task<(string failureMessage, LedgerWrapper? data)> Select(SelectLedgerRequest request, int loginUserID);
        Task<(string failureMessage, AdminLedgerWrapper? data)> AdminLedger(AdminLedgerRequest request, int userID);
        Task<(string failureMessage, AdminReconciliationWrapper? data)> AdminReconciliation(AdminLedgerRequest request, int userID, int userTypeID);
    }
}
