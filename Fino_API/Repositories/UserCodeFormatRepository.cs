using System.Data;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FintechApp.Repositories
{
    /// <summary>
    /// Store for the per-user-type code formats. Deliberately UNCACHED: the row carries the live
    /// running sequence, and a stale copy would hand two users the same code. Number allocation is
    /// a single conditional UPDATE … OUTPUT so the database row-lock — not app-level locking —
    /// serialises concurrent user creation, correctly across multiple app instances.
    /// </summary>
    public class UserCodeFormatRepository : IUserCodeFormatRepository
    {
        private readonly ApplicationDbContext _context;

        public UserCodeFormatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserCodeFormat>> GetAll() =>
            await _context.UserCodeFormats.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.UserTypeID)
                .ToListAsync();

        public async Task<UserCodeFormat?> GetByUserTypeID(int userTypeId) =>
            await _context.UserCodeFormats.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserTypeID == userTypeId && !x.IsDeleted);

        public async Task<int> Insert(UserCodeFormat entity)
        {
            await _context.UserCodeFormats.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(UserCodeFormat entity)
        {
            _context.UserCodeFormats.Update(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int userCodeFormatId)
        {
            var entity = await _context.UserCodeFormats.FindAsync(userCodeFormatId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }

        public async Task<int?> AllocateNextSequence(int userTypeId, string period)
        {
            // One statement: bump the counter and read back the number handed out. Within the same
            // period the counter moves forward (never below StartNumber); a new period restarts it
            // at StartNumber. 0 rows back = the type has no active format row.
            const string sql =
                "UPDATE dbo.UserCodeFormats " +
                "SET LastSequence = CASE WHEN SequencePeriod = @period " +
                "        THEN (CASE WHEN LastSequence < StartNumber THEN StartNumber - 1 ELSE LastSequence END) + 1 " +
                "        ELSE StartNumber END, " +
                "    SequencePeriod = @period, " +
                "    UpdatedateTime = SYSUTCDATETIME() " +
                "OUTPUT inserted.LastSequence " +
                "WHERE UserTypeID = @userTypeId AND IsDeleted = 0 AND IsActive = 1";

            var conn = _context.Database.GetDbConnection();
            IDbContextTransaction? current = _context.Database.CurrentTransaction;

            bool openedHere = false;
            if (conn.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
                openedHere = true;
            }

            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                if (current != null) cmd.Transaction = current.GetDbTransaction();
                AddParam(cmd, "@period", period ?? string.Empty);
                AddParam(cmd, "@userTypeId", userTypeId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync()) return reader.GetInt32(0);
                return null;
            }
            finally
            {
                if (openedHere && current == null)
                    await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<bool> UserCodeExists(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            return await _context.UserMasters.AsNoTracking().AnyAsync(x => x.AliasName == code);
        }

        public async Task<int> MaxSequenceInUse(int userTypeId, string prefix, string suffix, int sequenceLength)
        {
            var aliases = await _context.UserMasters.AsNoTracking()
                .Where(x => x.UserTypeID == userTypeId && x.AliasName != null && x.AliasName != string.Empty)
                .Select(x => x.AliasName)
                .ToListAsync();

            int max = 0;
            foreach (var alias in aliases)
            {
                int? seq = ExtractSequence(alias, prefix, suffix, sequenceLength);
                if (seq.HasValue && seq.Value > max) max = seq.Value;
            }
            return max;
        }

        /// <summary>The running number inside an existing code, or null when it isn't of this shape.</summary>
        private static int? ExtractSequence(string alias, string prefix, string suffix, int sequenceLength)
        {
            if (string.IsNullOrWhiteSpace(alias)) return null;
            if (!alias.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
            if (!alias.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return null;

            int start = prefix.Length;
            int length = alias.Length - suffix.Length - start;
            if (length <= 0) return null;
            // A code padded to 6 must not be read off a 4-digit legacy code of another shape.
            if (sequenceLength > 0 && length != sequenceLength && length < sequenceLength) return null;

            string digits = alias.Substring(start, length);
            return digits.All(char.IsDigit) && int.TryParse(digits, out int value) ? value : null;
        }

        private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
