namespace FintechApp.Responses.CallbackMaster
{
    public class CallbackMasterResponse
    {
        public int CallbackMasterID { get; set; }
        public int UserID { get; set; }
        public string? CallbackURL { get; set; }
        public bool IsDelete { get; set; }
        public int ServiceID { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

