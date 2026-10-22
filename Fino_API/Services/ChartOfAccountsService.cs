using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IChartOfAccountsService" />
    public class ChartOfAccountsService : IChartOfAccountsService
    {
        private readonly ApplicationDbContext _context;
        public ChartOfAccountsService(ApplicationDbContext context) => _context = context;

        public async Task<List<AcctAccount>> GetAccounts() =>
            await _context.AcctAccounts.AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Code).ToListAsync();
    }
}
