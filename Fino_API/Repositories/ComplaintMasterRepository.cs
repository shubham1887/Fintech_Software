using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ComplaintMasterRepository : IComplaintMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ComplaintMaster>> _listCache;

        public ComplaintMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ComplaintMaster>>();
        }

        public async Task<List<ComplaintMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllComplaintMasters, () =>
                _context.ComplaintMasters.AsNoTracking()
                    .Include(x => x.UserMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<ComplaintMaster>();
        }

        public async Task<List<ComplaintMaster>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<ComplaintMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ComplaintMasterID == id);
        }

        public async Task<int> Insert(ComplaintMaster entity)
        {
            await _context.ComplaintMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllComplaintMasters);
            return result;
        }

        public async Task<int> Update(ComplaintMaster entity)
        {
            // GetAll caches AsNoTracking entities WITH their User/Service/Operator navigations, so
            // _context.Update(entity) would mark that whole graph Modified and write back the
            // related master rows too (and can throw an identity conflict if the key is already
            // tracked). Copy scalars onto the tracked row instead — navigations are left alone.
            var tracked = await _context.ComplaintMasters.FindAsync(entity.ComplaintMasterID);
            if (tracked == null)
                _context.ComplaintMasters.Update(entity);
            else
                _context.Entry(tracked).CurrentValues.SetValues(entity);

            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllComplaintMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ComplaintMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllComplaintMasters);
            return result;
        }
    }
}
