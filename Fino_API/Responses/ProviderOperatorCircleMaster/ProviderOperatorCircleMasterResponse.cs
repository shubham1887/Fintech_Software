namespace FintechApp.Responses.ProviderOperatorCircleMaster {
    public class ProviderOperatorCircleMasterResponse {
        public int ProviderOperatorCircleMasterID { get; set; }
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public int StateID { get; set; }
        public string Opcode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
