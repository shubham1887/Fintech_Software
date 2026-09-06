using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests.CallMeCategoryMaster;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CallMeCategoryMasterRepository : ICallMeCategoryMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public CallMeCategoryMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CallMeCategoryMaster>> GetAllCallMeCategoryMasters()
        {
            return await _context.CallMeCategoryMaster
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CallMeCategoryMaster?> GetCallMeCategoryMasterById(int callMeCategoryMasterID)
        {
            return await _context.CallMeCategoryMaster
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CallMeCategoryMasterID == callMeCategoryMasterID && !c.IsDeleted);
        }

        public async Task<List<CallMeCategoryMaster>> GetCallMeCategoryMasters(GetCallMeCategoryMasterRequest request)
        {
            var query = _context.CallMeCategoryMaster
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (request.CallMeCategoryMasterID > 0)
                query = query.Where(c => c.CallMeCategoryMasterID == request.CallMeCategoryMasterID);

            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(c => c.Name.Contains(request.Name));

            var skip = (request.PageNumber - 1) * request.PageSize;
            return await query.Skip(skip).Take(request.PageSize).ToListAsync();
        }

        public async Task<int> Insert(CallMeCategoryMaster callMeCategoryMaster)
        {
            await _context.CallMeCategoryMaster.AddAsync(callMeCategoryMaster);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(CallMeCategoryMaster callMeCategoryMaster)
        {
            _context.CallMeCategoryMaster.Update(callMeCategoryMaster);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(CallMeCategoryMaster callMeCategoryMaster)
        {
            _context.CallMeCategoryMaster.Update(callMeCategoryMaster);
            return await _context.SaveChangesAsync();
        }
    }
}