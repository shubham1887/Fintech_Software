using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests.CallMeRequest;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CallMeRequestRepository : ICallMeRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public CallMeRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CallMeRequest>> GetAllCallMeRequests()
        {
            return await _context.CallMeRequests
                .Include(c => c.CallMeCategoryMaster)
                .Include(c => c.UserMaster)
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CallMeRequest?> GetCallMeRequestById(int callMeRequestID)
        {
            return await _context.CallMeRequests
                .Include(c => c.CallMeCategoryMaster)
                .Include(c => c.UserMaster)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CallMeRequestID == callMeRequestID && !c.IsDeleted);
        }

        public async Task<List<CallMeRequest>> GetCallMeRequests(GetCallMeRequestRequest request)
        {
            var query = _context.CallMeRequests
                .Include(c => c.CallMeCategoryMaster)
                .Include(c => c.UserMaster)
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (request.CallMeRequestID > 0)
                query = query.Where(c => c.CallMeRequestID == request.CallMeRequestID);

            if (request.UserID > 0)
                query = query.Where(c => c.UserID == request.UserID);

            if (request.UplineUserID > 0)
                query = query.Where(c => c.UplineUserID == request.UplineUserID);

            if (request.CallMeCategoryMasterID > 0)
                query = query.Where(c => c.CallMeCategoryMasterID == request.CallMeCategoryMasterID);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status == request.Status);

            var skip = (request.PageNumber - 1) * request.PageSize;
            return await query.Skip(skip).Take(request.PageSize).ToListAsync();
        }

        public async Task<int> Insert(CallMeRequest callMeRequest)
        {
            await _context.CallMeRequests.AddAsync(callMeRequest);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(CallMeRequest callMeRequest)
        {
            _context.CallMeRequests.Update(callMeRequest);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(CallMeRequest callMeRequest)
        {
            _context.CallMeRequests.Update(callMeRequest);
            return await _context.SaveChangesAsync();
        }
    }
}