namespace FintechApp.Responses.DMTBank
{
    public class DMTBankResponse
    {
        public int DMTBankID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DefaultIFSC { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsIMPS { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class DMTBankDropdownResponse
    {
        public int DMTBankID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DefaultIFSC { get; set; } = string.Empty;
    }
}
