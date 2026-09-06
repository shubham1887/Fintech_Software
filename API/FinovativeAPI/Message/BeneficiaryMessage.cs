namespace Project.Message
{
    public class BeneficiaryMessage
    {
        public static readonly string InvalidName = "Please enter valid Beneficiary name";
        public static readonly string InvalidID = "Please enter valid Beneficiary ID";
        public static readonly string InvalidBankName = "Please enter valid Bank name";
        public static readonly string InvalidAccountNo = "Please enter valid account number";
        public static readonly string InvalidIFSC = "Please enter valid IFSC";

        public static readonly string InvalidGUID = "Please enter valid guid";
        public static readonly string InvalidOTP = "Please enter valid OTP";
        public static readonly string InvalidMobile = "Please enter valid sender mobile";
        public static readonly string InvalidBeneMobile = "Please enter valid beneficiary mobile";
        public static readonly string VerifyOTP = "Please verify beneficiary OTP";
        public static readonly string InvalidPinCode = "Please enter valid pincode";
        public static readonly string InvalidCity = "Please  enter valid city";
        public static readonly string InvalidState = "Please  enter valid state";

        public static readonly string OTPSentOnMobileNumer = "OTP sent on sender number";

        public static readonly string NoBeneficiaryFound = "No beneficiary found";

        public static readonly string InvalidSender = "Please select valid sender";
        public static readonly string InvalidBeneficiary = "Please select valid beneficiary";
        public static readonly string InvalidPaymentType = "Please select valid payment type";
        public static readonly string InvalidUserID = "Please select valid User ID";

        public static readonly string UpdateSuccess = "Beneficiary Added successfully";
        public static readonly string AlreadyVerified = "Beneficiary already verified";
        public static readonly string AlreadyExits = "Beneficiary already exits";
        public static readonly string InsertFailure = "Beneficiary not added";
        public static readonly string DeleteFailure = "A/C Holder not deleted";
        public static readonly string DeleteSuccess = "A/C Holder deleted successfully";
        public static readonly string InsertSuccess = "Beneficiary added successfully";

        public static readonly string OTP = "[OTP] is otp for your beneficiary";

        public static readonly string NotFound = "Beneficiary not found";
        public static readonly string IFSCDetailsNotFound = "IFSC details not found";

        public static readonly string UpdateStatusSuccess = "Beneficiary status updated successfully";
        public static readonly string UpdateStatusfail = "Beneficiary status not updated";
        public static readonly string ActiveSuccess = "Beneficiary activated successfully";
        public static readonly string Activedeactfail = "Beneficiary not activated/deactivated ";
        public static readonly string DeactiveSuccess = "Beneficiary deactivated successfully";
        public static readonly string AccountLimitExceed = "Account limit exceed, please contact to customer care";
        public static readonly string BeneVerifySuccess = "Beneficiary Verified successfully";
        public static readonly string BeneRegistrationSuccess = "Request to link Bank Account has been submitted successfully";
        public static readonly string BeneOTP = "OTP generated successfully";
    }
}