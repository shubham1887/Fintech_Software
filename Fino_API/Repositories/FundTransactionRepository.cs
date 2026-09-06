using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.FundTransaction;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class FundTransactionRepository : IFundTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<FundTransaction>> _listCache;

        public FundTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<FundTransaction>>();
        }

        public async Task<List<FundTransaction>> GetAllFundTransactions()
        {
            return _listCache.GetOrCreate(CacheKeys.AllFundTransactions, () =>
            {
                return _context.FundTransactions
                    .Include(f => f.Sender)
                    .Include(f => f.Receiver)
                    .Include(f => f.WalletMaster)
                    .Include(f => f.RequestTo)
                    .Include(f => f.Mode)
                    .Include(f => f.SubMode)
                    .Where(f => f.IsDelete == false)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<FundTransaction>();
        }

        public async Task<FundTransaction?> GetFundTransactionById(int fundTransactionId)
        {
            var all = await GetAllFundTransactions();
            return all.FirstOrDefault(f => f.FundTransactionID == fundTransactionId);
        }

        public async Task<List<FundTransaction>> GetFundTransactions(GetFundTransactionRequest request)
        {
            var all = await GetAllFundTransactions();
            var query = all.AsQueryable();

            if (request.FundTransactionID > 0)
                query = query.Where(f => f.FundTransactionID == request.FundTransactionID);

            if (request.SenderID > 0)
                query = query.Where(f => f.SenderID == request.SenderID);

            if (request.ReceiverID > 0)
                query = query.Where(f => f.ReceiverID == request.ReceiverID);

            if (request.WalletID > 0)
                query = query.Where(f => f.WalletID == request.WalletID);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(f => f.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));

            if (request.FromDate.HasValue)
                query = query.Where(f => f.TransactionDOC >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(f => f.TransactionDOC <= request.ToDate.Value);

            return query.ToList();
        }

        public async Task<int> Insert(FundTransaction fundTransaction)
        {
            await _context.FundTransactions.AddAsync(fundTransaction);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Update(FundTransaction fundTransaction)
        {
            _context.FundTransactions.Update(fundTransaction);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Delete(FundTransaction fundTransaction)
        {
            _context.FundTransactions.Update(fundTransaction);  // Soft delete
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> GetPendingRequestCount(int requestToId)
        {
            var all = await GetAllFundTransactions();
            return all.Count(f => f.RequestToID == requestToId && f.Status == "PROCESS" && f.Type == "TRANSFER_REQUEST");
        }

        public async Task<int> GetPendingReverseRequestCount(int requestToId)
        {
            var all = await GetAllFundTransactions();
            return all.Count(f => f.RequestToID == requestToId && f.Status == "PROCESS" && f.Type == "REVERSE_REQUEST");
        }

        private void ClearCache() => _listCache.Remove(CacheKeys.AllFundTransactions);
    }
}