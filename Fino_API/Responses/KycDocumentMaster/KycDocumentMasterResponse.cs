namespace FintechApp.Responses.KycDocumentMaster
{
    public class KycDocumentMasterResponse
    {
        public int KycDocumentMasterID { get; set; }
        public string? Label { get; set; }
        public string? DocumentType { get; set; }
        public string? Remark { get; set; }
        public bool IsRequired { get; set; }
        public string? RegularRxpression { get; set; }
        public string? SampleImageURL { get; set; }
    }
}
