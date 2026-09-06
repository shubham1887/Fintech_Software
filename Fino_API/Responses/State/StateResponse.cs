namespace FintechApp.Responses.State
{
    public class StateResponse
    {
        public int StateID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class StateDropdownResponse
    {
        public int StateID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }
}
