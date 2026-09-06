namespace FintechApp.Responses.UserOperatorLimit
{
    public class UserOperatorLimitResponse
    {
        public int UserOperatorLimitID { get; set; }
        public int UserID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public decimal LimitAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

