using System.Data;
using ClosedXML.Excel;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Reconciliation;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old ReconBal.Upload. Parses an uploaded provider reconciliation file (.xlsx/.csv),
    // matches each row to a TransactionMaster by ProviderRequestID, and settles via the
    // AccountProcessing engine: SUCCESS → SuccessTransaction, FAILED → RefundTransaction.
    public class ReconService : IReconService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly IServiceKindResolver _serviceKinds;
        private readonly Logger _logger;

        public ReconService(ITransactionMasterRepository txnRepo, IAccountCoreProcessingService accountCore, IServiceKindResolver serviceKinds)
        {
            _txnRepo = txnRepo;
            _accountCore = accountCore;
            _serviceKinds = serviceKinds;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, int successfulUpdates, int totalRecords)> Upload(ReconUploadRequest request, int currentUserID)
        {
            if (request?.ExcelDocument == null || string.IsNullOrWhiteSpace(request.ExcelDocument.Data))
                return (Messages.Recon.Message.NoFile, 0, 0);

            string ext = System.IO.Path.GetExtension(request.ExcelDocument.Name ?? string.Empty).ToLowerInvariant();

            byte[] bytes;
            try { bytes = Convert.FromBase64String(request.ExcelDocument.Data); }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Recon.Message.FileUnreadable, 0, 0); }

            List<ReconRow> rows;
            try
            {
                rows = ext switch
                {
                    ".xlsx" => ParseXlsx(bytes),
                    ".csv" => ParseCsv(bytes),
                    _ => new List<ReconRow>()
                };
            }
            catch (Exception ex) { _logger.Write(ex); return (Messages.Recon.Message.FileUnreadable, 0, 0); }

            if (ext != ".xlsx" && ext != ".csv")
                return (Messages.Recon.Message.UnsupportedFormat, 0, 0);

            if (rows.Count == 0)
                return (Messages.Recon.Message.FileEmpty, 0, 0);

            // Validate required fields
            var errors = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                int n = i + 1;
                if (string.IsNullOrWhiteSpace(r.RequestID) || string.IsNullOrWhiteSpace(r.Status))
                    errors.Add($"Row {n}: RequestID and Status are required.");
                else if (!r.Status.EqualsIgnoreCase(StatusType.Success) && !r.Status.EqualsIgnoreCase(StatusType.Failed))
                    errors.Add($"Row {n}: Status must be SUCCESS or FAILED.");
                else if (r.Status.EqualsIgnoreCase(StatusType.Success) && (r.ReferanceNo == null || r.ReferanceNo.Length <= 5))
                    errors.Add($"Row {n}: a valid reference number is required for SUCCESS.");
            }
            if (errors.Count > 0)
                return (string.Join(Environment.NewLine, errors), 0, rows.Count);

            int successfulUpdates = 0;
            foreach (var r in rows)
            {
                try
                {
                    var txn = await _txnRepo.GetByProviderRequestID(r.RequestID);
                    if (txn == null) continue;

                    if (r.Status.EqualsIgnoreCase(StatusType.Success))
                    {
                        // Only finalise transactions still in process/pending.
                        if (!txn.Status.EqualsIgnoreCase(StatusType.Pending)) continue;
                        // AEPS/MATM are cash-in — never auto-success via recon.
                        if (await _serviceKinds.IsCashInService(txn.ServiceID)) continue;
                        await _accountCore.SuccessTransaction(txn.TransactionMasterID, r.ReferanceNo, "Reconciliation success");
                        successfulUpdates++;
                    }
                    else // FAILED
                    {
                        // Don't re-fail an already-failed txn; skip DMT (handled by its own refund flow).
                        if (txn.Status.EqualsIgnoreCase(StatusType.Failed)) continue;
                        if (await _serviceKinds.IsDMTService(txn.ServiceID)) continue;
                        string r2 = await _accountCore.RefundTransaction(txn.TransactionMasterID, "Reconciliation failure", DOCTypeConstants.Recharge);
                        if (r2.EqualsIgnoreCase(StatusType.Ok)) successfulUpdates++;
                    }
                }
                catch (Exception ex) { _logger.Write(ex); }
            }

            return (StatusType.Ok, successfulUpdates, rows.Count);
        }

        private static List<ReconRow> ParseXlsx(byte[] bytes)
        {
            var result = new List<ReconRow>();
            using var ms = new MemoryStream(bytes);
            using var wb = new XLWorkbook(ms);
            var ws = wb.Worksheet(1);

            var used = ws.RangeUsed();
            if (used == null) return result;

            // Map header names → column numbers from the first row.
            var header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var firstRow = used.FirstRow();
            foreach (var cell in firstRow.Cells())
                header[cell.GetString().Trim()] = cell.Address.ColumnNumber;

            int ColOf(string name) => header.TryGetValue(name, out int c) ? c : -1;
            int cReq = ColOf("RequestID"), cStatus = ColOf("Status"), cRef = ColOf("ReferanceNo"), cDate = ColOf("Date"), cAmt = ColOf("Amount");

            foreach (var row in used.Rows().Skip(1))
            {
                string Get(int col) => col > 0 ? row.Cell(col).GetString().Trim() : string.Empty;
                var rr = new ReconRow
                {
                    RequestID = Get(cReq),
                    Status = Get(cStatus),
                    ReferanceNo = Get(cRef),
                    Date = Get(cDate),
                    Amount = Get(cAmt)
                };
                if (!string.IsNullOrWhiteSpace(rr.RequestID))
                    result.Add(rr);
            }
            return result;
        }

        private static List<ReconRow> ParseCsv(byte[] bytes)
        {
            var result = new List<ReconRow>();
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return result;

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
            int Idx(string name) => headers.FindIndex(h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
            int cReq = Idx("RequestID"), cStatus = Idx("Status"), cRef = Idx("ReferanceNo"), cDate = Idx("Date"), cAmt = Idx("Amount");

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                string Get(int idx) => idx >= 0 && idx < parts.Length ? parts[idx].Trim() : string.Empty;
                var rr = new ReconRow
                {
                    RequestID = Get(cReq),
                    Status = Get(cStatus),
                    ReferanceNo = Get(cRef),
                    Date = Get(cDate),
                    Amount = Get(cAmt)
                };
                if (!string.IsNullOrWhiteSpace(rr.RequestID))
                    result.Add(rr);
            }
            return result;
        }

        private sealed class ReconRow
        {
            public string RequestID { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string ReferanceNo { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Amount { get; set; } = string.Empty;
        }
    }
}
