namespace FintechApp.Requests.AgentDetails
{
    public class AgentUploadRequest
    {
        public string ServiceType { get; set; } = string.Empty;
        public ExcelDocumentModel ExcelDocument { get; set; } = new();
    }

    public class ExcelDocumentModel
    {
        public string Data { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class SelectPincodeRequest
    {
        public string PinCode { get; set; } = string.Empty;
    }
}
