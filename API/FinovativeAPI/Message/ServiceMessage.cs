namespace Project.Message
{
    public class ServiceMessage
    {
        public static readonly string InvalidName = "Please enter valid service name";
        public static readonly string Invalid = "Please select valid service";
        public static readonly string UpdateSuccess = "Service updated successfully";
        public static readonly string InvalidCharge = "Please enter valid charge amount";
        public static readonly string InvalidMinBalance = "Please enter valid charge min. balance";
        public static readonly string NameAlreadyExits = "Service name already exits";
        public static readonly string InsertFailure = "Service not added";
        public static readonly string InsertSuccess = "Service added successfully";

        public static readonly string UpdateFail = "Service not updated";
        public static readonly string NotFound = "Requested Service not found";
        public static readonly string Off = "Service is off, please contact to customer care";

        public static readonly string InvalidServideID = "Invalid Service";
        public static readonly string InvalidUserID = "Invalid User";
        public static readonly string InvalidOnetime = "Invalid one time change";
        public static readonly string InvalidDaily = "Invalid daily change";
        public static readonly string InvalidMonthly = "Invalid monthly charge";
        public static readonly string DeleteChargeSucess = "Delete service charge successfully";
        public static readonly string DeleteChargeFail = "Unable to Delete";
        public static readonly string InsertChargeFailure = "Unable to insert";
        public static readonly string DuplicateChargeFailure = "Already charge exists for this user";
        public static readonly string InsertChargeSuccess = "Service charge insert successfully";
        public static readonly string OtpVerifty = "OTP verification successful";
    }
}