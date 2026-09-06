namespace Project.Message
{
    public class AgentMessage
    {
        public static readonly string InvalidName = "Please enter valid sender name, length must be at least 4 characters";

        public static readonly string InvalidMobile = "Please enter valid  mobile";
        public static readonly string InvalidFirmName = "Please enter valid  firm name";
        public static readonly string InvalidPAN = "Please enter valid  PAN";
        public static readonly string InvalidAadhar = "Please enter valid  aadhar number";
        public static readonly string InvalidAddress = "Please enter valid  address";
        public static readonly string InvalidCity = "Please enter valid  city";
        public static readonly string InvalidPincode = "Please enter valid  pincode";
        public static readonly string NotRegistered = "Agent not registered";

        public static readonly string InvalidUserID = "Please select valid User ID";

        public static readonly string UpdateSuccess = "Agent Verified successfully";
        public static readonly string AlreadyVerified = "Agent already verified";
        public static readonly string MobileAlreadyExits = "Agent mobile already exits";
        public static readonly string InsertFailure = "Agent not added";
        public static readonly string InsertSuccess = "Agent added successfully";
    }
}