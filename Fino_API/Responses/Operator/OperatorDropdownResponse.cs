namespace FintechApp.Responses.Operator
{
    public class OperatorDropdownResponse
    {
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Opcode { get; set; } = string.Empty;
        public string OperatorImagePath { get; set; } = string.Empty;
    }
}
