using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.Operator;
using FintechApp.Responses.Operator;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorMaster>> _listCache;

        public OperatorRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<OperatorMaster>>();
        }

        public async Task<List<OperatorMaster>> GetAllOperators()
        {
            return _listCache.GetOrCreate(CacheKeys.AllOperators, () =>
            {
                return _context.OperatorMasters
                    .Include(o => o.ServiceMaster)
                    .Where(o => o.IsDeleted == false && o.IsDelete != true)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<OperatorMaster>();
        }

        public async Task<OperatorMaster?> GetOperatorById(int operatorId)
        {
            var all = await GetAllOperators();
            return all.FirstOrDefault(o => o.OperatorID == operatorId);
        }

        public async Task<OperatorMaster?> GetOperatorByOpcode(string opcode)
        {
            var all = await GetAllOperators();
            return all.FirstOrDefault(o => o.Opcode.Equals(opcode, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<OperatorMaster>> GetOperators(GetOperatorRequest request)
        {
            var all = await GetAllOperators();
            var query = all.AsQueryable();

            if (request.OperatorID > 0)
                query = query.Where(o => o.OperatorID == request.OperatorID);

            if (request.ServiceID > 0)
                query = query.Where(o => o.ServiceID == request.ServiceID);

            if (!string.IsNullOrEmpty(request.OperatorName))
                query = query.Where(o => o.OperatorName.Contains(request.OperatorName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Opcode))
                query = query.Where(o => o.Opcode.Equals(request.Opcode, StringComparison.OrdinalIgnoreCase));

            if (request.IsActive.HasValue)
                query = query.Where(o => o.IsActive == request.IsActive.Value);

            return query.ToList();
        }

        public async Task<int> Insert(OperatorMaster operatorMaster)
        {
            await _context.OperatorMasters.AddAsync(operatorMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearOperatorCache();

            return result;
        }

        public async Task<int> Update(OperatorMaster operatorMaster)
        {
            // The cached list is loaded with Include(ServiceMaster); re-attaching that stale
            // navigation via Update() marks the SERVICE row Modified too and overwrites it
            // with old values. Detach it so only the operator row is written.
            operatorMaster.ServiceMaster = null!;
            _context.OperatorMasters.Update(operatorMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearOperatorCache();

            return result;
        }

        public async Task<int> Delete(OperatorMaster operatorMaster)
        {
            operatorMaster.ServiceMaster = null!;
            _context.OperatorMasters.Update(operatorMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearOperatorCache();

            return result;
        }

        public async Task<List<OperatorMaster>> GetActiveOperators()
        {
            var all = await GetAllOperators();
            return all
                .Where(o => o.IsActive)
                .OrderBy(o => o.OperatorName)
                .ToList();
        }

        public async Task<List<OperatorDropdownResponse>> GetDropDownOperators()
        {
            var allOperators = await GetActiveOperators();
            return allOperators
                .Select(o => new OperatorDropdownResponse
                {
                    OperatorID = o.OperatorID,
                    OperatorName = o.OperatorName,
                    Opcode = o.Opcode,
                    OperatorImagePath = o.OperatorImagePath
                }).ToList();
        }

        public void ClearOperatorCache()
        {
            _listCache.Remove(CacheKeys.AllOperators);
        }
    }
}