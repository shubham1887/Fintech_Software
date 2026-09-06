namespace FintechApp.Responses.NPCIBank
{
    public class NPCIBankResponse
    {
        public int NPCIBankID { get; set; }
        public string? BankCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? IFSC { get; set; }
        public string? IIN { get; set; }
        public string? ParentBank { get; set; }
        public string? BankType { get; set; }
        public bool IsActive { get; set; }
    }
}
