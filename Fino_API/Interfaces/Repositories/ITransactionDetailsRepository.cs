using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionDetailsRepository
    {
        Task<List<TransactionDetails>> GetAll();

        // Ledger rows from a date onwards, straight from the DB. Panel-wide balance maths needs
        // every user's postings, and GetAll() hands back the whole (cached) table to do it — this
        // keeps that work bounded to the period actually being reported on.
        Task<List<TransactionDetails>> GetFromDate(DateTime from);

        // Every ledger row posted against one transaction, straight from the DB — deliberately NOT
        // through the cached GetAll(). The refund path calls this to see which legs actually landed,
        // moments after they were written, and a stale cache there would answer "that leg was never
        // posted" about a leg that was.
        Task<List<TransactionDetails>> GetByReference(int referenceID);

        Task<TransactionDetails?> GetByID(int id);
        Task<List<TransactionDetails>> GetByUserID(int userid);
        Task<int> Insert(TransactionDetails entity);
        Task<int> Update(TransactionDetails entity);
        Task<int> Delete(int id);
    }
}
