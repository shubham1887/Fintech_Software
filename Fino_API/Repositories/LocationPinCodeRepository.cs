using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LocationPinCodeRepository : ILocationPinCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationPinCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LocationPinCodeMaster?> GetByPinCode(string pinCode)
        {
            return await _context.LocationPinCodeMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PINCODE == pinCode && x.IsActive && !x.IsDelete);
        }

        public async Task<bool> CheckPinCode(string pinCode)
        {
            return !await _context.LocationPinCodeMasters.AnyAsync(x => x.PINCODE == pinCode);
        }

        public async Task<int> Insert(LocationPinCodeMaster entity)
        {
            await _context.LocationPinCodeMasters.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
