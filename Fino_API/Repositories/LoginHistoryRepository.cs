using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoginHistory>> GetByUserID(int userId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
        {
            var query = _context.LoginHistories.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0)
                query = query.Where(x => x.UserID == userId);

            if (fromDate.HasValue)
                query = query.Where(x => x.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.Date <= toDate.Value);

            return await query
                .OrderByDescending(x => x.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> Insert(LoginHistory entity)
        {
            await _context.LoginHistories.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
