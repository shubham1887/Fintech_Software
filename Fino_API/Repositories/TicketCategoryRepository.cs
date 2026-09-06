using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.TicketCategory;
using FintechApp.Responses.TicketCategory;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class TicketCategoryRepository : ITicketCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TicketCategory>> _listCache;

        public TicketCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<TicketCategory>>();
        }

        public async Task<List<TicketCategory>> GetAllTicketCategories()
        {
            return _listCache.GetOrCreate(CacheKeys.AllTicketCategories, () =>
            {
                return _context.TicketCategories
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<TicketCategory>();
        }

        public async Task<TicketCategory?> GetTicketCategoryById(int ticketCategoryId)
        {
            var all = await GetAllTicketCategories();
            return all.FirstOrDefault(t => t.TicketCategoryID == ticketCategoryId);
        }

        public async Task<TicketCategory?> GetTicketCategoryByName(string name)
        {
            var all = await GetAllTicketCategories();
            return all.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<TicketCategory>> GetTicketCategories(GetTicketCategoryRequest request)
        {
            var all = await GetAllTicketCategories();
            var query = all.AsQueryable();

            if (request.TicketCategoryID > 0)
                query = query.Where(t => t.TicketCategoryID == request.TicketCategoryID);

            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(t => t.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        public async Task<int> Insert(TicketCategory ticketCategory)
        {
            await _context.TicketCategories.AddAsync(ticketCategory);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Update(TicketCategory ticketCategory)
        {
            _context.TicketCategories.Update(ticketCategory);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Delete(TicketCategory ticketCategory)
        {
            _context.TicketCategories.Remove(ticketCategory);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<List<TicketCategoryDropdownResponse>> GetDropDownTicketCategories()
        {
            var all = await GetAllTicketCategories();
            return all
                .OrderBy(t => t.Name)
                .Select(t => new TicketCategoryDropdownResponse
                {
                    TicketCategoryID = t.TicketCategoryID,
                    Name = t.Name
                }).ToList();
        }

        private void ClearCache() => _listCache.Remove(CacheKeys.AllTicketCategories);
    }
}