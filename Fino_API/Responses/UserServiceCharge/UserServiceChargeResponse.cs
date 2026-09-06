namespace FintechApp.Responses.UserServiceCharge
{
    public class UserServiceChargeResponse
    {
        public int UserServiceChargeID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public decimal Charge { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

