using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    /// <summary>
    /// Deliberately un-cached: an approval queue must reflect inserts/approvals immediately, so every
    /// read hits the DB. (The cached repos in this codebase need an API restart after a direct write —
    /// we don't want that behaviour for a live queue.)
    /// </summary>
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly ApplicationDbContext _context;

        public PendingRegistrationRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<PendingRegistration>> GetAll() =>
            await _context.PendingRegistrations.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.PendingRegistrationID)
                .ToListAsync();

        public async Task<List<PendingRegistration>> GetByStatus(string status) =>
            await _context.PendingRegistrations.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status == status)
                .OrderByDescending(x => x.PendingRegistrationID)
                .ToListAsync();

        public async Task<PendingRegistration?> GetByID(int id) =>
            await _context.PendingRegistrations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PendingRegistrationID == id && !x.IsDeleted);

        public async Task<int> Insert(PendingRegistration entity)
        {
            await _context.PendingRegistrations.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(PendingRegistration entity)
        {
            _context.PendingRegistrations.Update(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<string?> FindDuplicateInUsers(string mobile, string email, string pan, string aadhaar, string gst)
        {
            mobile = (mobile ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            pan = (pan ?? string.Empty).Trim();
            aadhaar = (aadhaar ?? string.Empty).Trim();
            gst = (gst ?? string.Empty).Trim();

            var q = _context.UserMasters.AsNoTracking().Where(u => !u.IsDeleted);

            if (mobile.Length > 0 && await q.AnyAsync(u => u.MobileNumber == mobile))
                return "Mobile number";
            if (email.Length > 0 && await q.AnyAsync(u => u.EmailID == email))
                return "Email address";
            if (pan.Length > 0 && await q.AnyAsync(u => u.PANNo == pan))
                return "PAN";
            if (aadhaar.Length > 0 && await q.AnyAsync(u => u.AadharNo == aadhaar))
                return "Aadhaar number";
            if (gst.Length > 0 && await q.AnyAsync(u => u.GSTNo == gst))
                return "GST number";
            return null;
        }

        public async Task<string?> FindDuplicateInPending(string mobile, string email, string pan, string aadhaar, string gst)
        {
            mobile = (mobile ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            pan = (pan ?? string.Empty).Trim();
            aadhaar = (aadhaar ?? string.Empty).Trim();
            gst = (gst ?? string.Empty).Trim();

            // Only still-pending rows block a re-apply; rejected ones may re-register.
            var q = _context.PendingRegistrations.AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == "Pending");

            if (mobile.Length > 0 && await q.AnyAsync(p => p.MobileNumber == mobile))
                return "Mobile number";
            if (email.Length > 0 && await q.AnyAsync(p => p.EmailID == email))
                return "Email address";
            if (pan.Length > 0 && await q.AnyAsync(p => p.PANNo == pan))
                return "PAN";
            if (aadhaar.Length > 0 && await q.AnyAsync(p => p.AadharNo == aadhaar))
                return "Aadhaar number";
            if (gst.Length > 0 && await q.AnyAsync(p => p.GSTNo == gst))
                return "GST number";
            return null;
        }
    }
}
