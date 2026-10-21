using FintechApp.Data;
using FintechApp.Interfaces.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;  // GetDbTransaction()

namespace FintechApp.Services
{
    /// <summary>
    /// See <see cref="ITaxDocumentNumberService"/>. The whole correctness of this class is in one
    /// statement: an UPDATE that increments the counter and returns the new value in the same
    /// breath, so no two callers can read the same number.
    /// </summary>
    public class TaxDocumentNumberService : ITaxDocumentNumberService
    {
        private readonly ApplicationDbContext _context;

        public TaxDocumentNumberService(ApplicationDbContext context) => _context = context;

        /// <summary>
        /// India's financial year runs April to March, so 2026-09-30 is FY 2026-27 -> "2627",
        /// and 2026-02-28 is still FY 2025-26 -> "2526".
        /// </summary>
        public string FinancialYearOf(DateTime date)
        {
            int startYear = date.Month >= 4 ? date.Year : date.Year - 1;
            return $"{startYear % 100:D2}{(startYear + 1) % 100:D2}";
        }

        public async Task<(string DocumentNo, string FailureMessage)> Allocate(
            string seriesCode, string companyGstin, DateTime documentDate)
        {
            if (string.IsNullOrWhiteSpace(seriesCode)) return (string.Empty, "Series code is required.");
            if (string.IsNullOrWhiteSpace(companyGstin)) return (string.Empty, "Company GSTIN is required.");

            string fy = FinancialYearOf(documentDate);

            // ONE statement: increment and read back. UPDATE takes an exclusive lock on the row for
            // its duration, so a concurrent caller waits and then reads the already-incremented
            // value. A SELECT-then-UPDATE here would be the classic race, and the bug it produces
            // (two documents, one number) is invisible until a return is filed.
            const string sql = @"
UPDATE dbo.TaxDocumentSeries
   SET CurrentNumber = CurrentNumber + 1,
       UpdatedateTime = SYSUTCDATETIME()
OUTPUT inserted.CurrentNumber, inserted.Prefix, inserted.NumberLength, inserted.FinancialYear
 WHERE SeriesCode = @code
   AND CompanyGSTIN = @gstin
   AND FinancialYear = @fy
   AND IsActive = 1
   AND IsDeleted = 0;";

            var rows = new List<(int Number, string Prefix, int Length, string Fy)>();

            var conn = _context.Database.GetDbConnection();
            bool openedHere = conn.State != System.Data.ConnectionState.Open;
            if (openedHere) await conn.OpenAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                // The ambient transaction, when the caller has one open, so the allocation commits
                // or rolls back with the document rather than drifting from it.
                var tx = _context.Database.CurrentTransaction;
                if (tx != null) cmd.Transaction = tx.GetDbTransaction();

                cmd.Parameters.Add(new SqlParameter("@code", seriesCode));
                cmd.Parameters.Add(new SqlParameter("@gstin", companyGstin));
                cmd.Parameters.Add(new SqlParameter("@fy", fy));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3)));
                }
            }
            finally
            {
                if (openedHere) await conn.CloseAsync();
            }

            if (rows.Count == 0)
                return (string.Empty, $"No active series '{seriesCode}' for GSTIN {companyGstin} in FY {fy}. "
                                   + "Open the series for this financial year before issuing documents.");

            // The unique index on (SeriesCode, CompanyGSTIN, FinancialYear) makes this impossible,
            // but if it is ever dropped, two counters for one series would issue duplicates — so
            // refuse rather than pick one.
            if (rows.Count > 1)
                return (string.Empty, $"Series '{seriesCode}' has {rows.Count} rows for FY {fy}. "
                                   + "That is two counters for one series; fix the master before issuing.");

            var row = rows[0];
            return Format(row.Prefix, row.Fy, row.Number, row.Length);
        }

        /// <summary>
        /// Build and validate the printed number. Static and free of the database so the format
        /// rules can be exercised directly: GST caps a document number at 16 characters and allows
        /// only letters, digits, "/" and "-", and a number the portal rejects is worse than one
        /// that is merely ugly — so it is checked on the way out, not discovered at filing.
        /// </summary>
        public static (string DocumentNo, string FailureMessage) Format(string prefix, string fy, int number, int length)
        {
            string documentNo = $"{prefix}{fy}/{number.ToString().PadLeft(Math.Max(1, length), '0')}";

            if (documentNo.Length > 16)
                return (string.Empty, $"Generated number '{documentNo}' is {documentNo.Length} characters; "
                                   + "GST allows 16. Shorten the series prefix or the number length.");

            foreach (char c in documentNo)
            {
                if (!char.IsLetterOrDigit(c) && c != '/' && c != '-')
                    return (string.Empty, $"Generated number '{documentNo}' contains '{c}'. "
                                       + "GST allows only letters, digits, '/' and '-'.");
            }

            return (documentNo, string.Empty);
        }
    }
}
