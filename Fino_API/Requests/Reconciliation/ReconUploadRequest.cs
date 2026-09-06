namespace FintechApp.Requests.Reconciliation
{
    // Base64-encoded file payload (matches old CustomFile: Name + Data).
    public class ReconFile
    {
        public string Name { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // base64 content
    }

    // Provider reconciliation upload. ExcelDocument carries an .xlsx or .csv with columns:
    // Date (yyyyMMdd), RequestID (= TransactionMaster.ProviderRequestID), Amount, Status (SUCCESS/FAILED), ReferanceNo.
    public class ReconUploadRequest
    {
        public ReconFile ExcelDocument { get; set; } = new ReconFile();
        public string ServiceType { get; set; } = string.Empty;
    }
}
