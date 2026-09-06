using System;

namespace Project.Message
{
    public class UserMessage
    {
        public static readonly string ParentDeactivate = "Parent deactivated, please contact customer care";
        public static readonly string InvalidChannelUser = "Sorry, you cannot make as a channel user.";
        public static readonly string UploadSuccess = "Profile uploaded successfully";
        public static readonly string ReverseWalletBal = "Please reverse all wallet balance then delete user";
        public static readonly string UploadFail = "Profile upload fail";
        public static readonly string SignUpType = "SignUp";
        public static readonly string KYCNotApproved = "Your KYC is not approved, please contact to customer care";
        public static readonly string Registrationfailure = "User not registered.";
        public static readonly string RegistrationSuccess = "User registered successfully.";
        public static readonly string DeleteSuccess = "User deleted successfully.";
        public static readonly string RekycSuccess = "User rekyc successfully.";
        public static readonly string RekycFail = "Rekyc fail";
        public static readonly string DeleteFail = "User not deleted";
        public static readonly string UpdateSuccess = "User updated successfully";
        public static readonly string CommissioholdSuccess = "Commission hold updated successfully";
        public static readonly string UpdatePasswordSuccess = "Password change successfully";
        public static readonly string UpdatepassFail = "Password not updated";
        public static readonly string UpdatePinSuccess = "Pin change successfully";
        public static readonly string UpdatePinFail = "Pin not updated";
        public static readonly string PinAlreadyUsed = "Pin already used";
        public static readonly string PinChangeLimitReached = "Pin change limite reached";
        public static readonly string AppLock = "Contact customer service for assistance with the PIN or app being locked";


        public static readonly string UpdateOutletIDFail = "OutletID not updated";

        public static readonly string UpdateProfileSuccess = "Profile update successfully";
        public static readonly string UpdateProfileFail = "Profile not updated";
        public static readonly string UpdateFail = "User not updated";
        public static readonly string UserNotFound = "Requested user not found";
        public static readonly string NoKycPending = "No kyc pending for approval";

        public static readonly string TokenNotFound = "Token not found";
        public static readonly string Only1Mb = "Only upload image up to 1MB";

        public static readonly string LoginFailure = "Login failed";
        public static readonly string LoginSuccess = "Login successful";
        public static readonly string IsInitialPassword = "IsInitialPassword";

        public static readonly string DuplicateFirmName = "Firm name already exists.";
        public static readonly string DuplicateVirtualAccountNumber = "Account number  already exists.";
        public static readonly string DuplicateAliasName = "Alias Name already exists.";
        public static readonly string DuplicateMobileNumber = "Mobile number already exists.";
        public static readonly string DuplicateWhatsAppNumber = "Whatsapp number already exists.";
        public static readonly string DuplicateEmailID = "Email id already exists.";

        public static readonly string InvalidUserNameOrPassword = "Invalid user-name or password";

        public static readonly string UpdateToLatestVersion = "Please update to latest [platform] app version";
        public static readonly string InvalidAadhar = "Please enter valid aadhaar number";
        public static readonly string InvalidOTP = "Please enter valid OTP";
        public static readonly string InvalidMaxOs = "Please enter valid max outstanding amount";

        public static readonly string InvalidFigerPrint = "Please send valid Fingerprint";
        public static readonly string InvalidCaptcha = "Please send valid Captcha";

        public static readonly string InvalidUser = "Please select valid user";
        public static readonly string InvalideKYC = "You have already kyc done, no need kyc";
        public static readonly string InvalidState = "Please select valid state";
        public static readonly string InvalidData = "Invalid data";
        public static readonly string DataNotFound = "Data not found";

        public static readonly string UserNotOnboarded = "User not onboarded";

        public static readonly string UserAlreadyOnboarded = "User already onboarded";

        public static readonly string InvalidParent = "Please select valid parent";
        public static readonly string InvalidUserType = "Please select user type";

        public static readonly string ActivateSuccess = "User activated successfully.";
        public static readonly string ActivateFail = "User not activated";
        public static readonly string TokenSuccess = "Token generated successfully";
        public static readonly string TokenFail = "Token generation Fail";

        public static readonly string DeactivateSuccess = "User deactivated successfully.";
        public static readonly string DeactivateFail = "User not deactivated";
        public static readonly string DeactivatedUser = "Your account deactivated, please contact to admin";

        public static readonly string ForgotPasswordSuccess = "Password reset successfully";
        public static readonly string ForgotPasswordOTP = "OTP sent on your mobile number";
        public static readonly string SignUpOTP = "OTP sent on your number";
        public static readonly string SignUpVerifyOTP = "OTP verified successfully";
        public static readonly string ForgotPasswordFailure = "Password not reset.";
        public static readonly string ForgotPasswordMessage = "Your new password is [Password]";

        public static readonly string ForgotPasswordType = "ForgotPassword";
        public static readonly string InvalidUserName = "Please enter valid user-name";
        public static readonly string InvalidPersonName = "Please enter valid person name";
        public static readonly string InvalidPassword = "Please enter valid password";
        public static readonly string InvalidFirmName = "Please enter valid  firm name";
        public static readonly string InvalidAliasName = "Please enter valid alias name less than 12 Char";
        public static readonly string InvalidMobileNumber = "Please enter valid mobile number";
        public static readonly string InvalidEmailID = "Please enter valid email address";
        public static readonly string InvalidAddress = "Please enter valid address";
        public static readonly string InvalidPincode = "Please enter valid pincode";
        public static readonly string InvalidPan = "Please enter valid pan no";




        public static readonly string EmailNotAdded = "Email id not added";

        public static readonly string InvalidOldPass = "Invalid old password";
        public static readonly string InvalidNewPass = "Invalid new password";
        public static readonly string InvalidConfirmPass = "Invalid confirm password";

        public static readonly string InvalidConfirmPin = "Invalid confirm pin";
        public static readonly string InvalidOldPin = "Invalid old pin";
        public static readonly string InvalidPinAndPass = "check your old pin and password";
        public static readonly string InvalidNewPin = "Invalid new pin";
        public static readonly string InvalidUserID = "Invalid user";
        public static readonly string Invalid2FAPasscode = "Please enter valid security code";

        public static readonly string ShouldDifferentNewPassword = " Should different new password";

        public static readonly string InvalidMobileNumberAndToken = "Please enter valid mobile number and API Token";

        public static string RequestFromInvalidIP = "Request from invalid IP:";

        public static string IswebActive = "Web access activated successfully ";
        public static string IswebDeActive = "Web access deactivated successfully ";
        public static string IsAppActive = "APP access activated successfully ";
        public static string IsAppDeActive = "APP access deactivated successfully ";

        internal static string InvalidPin = "Please enter valid pin";
        internal static string InvalidPinCode = "Please enter valid Pincode";
        internal static string PinValidation = "Please choose a more secure PIN. Avoid simple patterns like 1234 or 1111";

        public static string IsInvoiceActive = "Invoice access activated successfully";
        public static string IsInvoiceDeActive = "Invoice access deactivated successfully";

        public static readonly string InvalidProfileImage = "Invalid profile image";

        public static readonly string ComplexPass = "Password must contains\n 1 capital alphabet,\n 1 small alphabet,\n 1 digit and 1 special character (minimum length 6 char.)";

        public static readonly string InvalidRequestID = "Please provide valid request id";
        public static readonly string DuplicateRequestID = "duplicate request id";
        public static readonly string NotFoundOrActive = "User not found/active";

        public static string InvoiceNotApplicable = "Invoice is not applicable for this user";

        public static string InvalidGST = "This user doesn't have GST number";

        public static string No = "no";

        public static readonly string MaskedPasswordAndOTPDefaultValue = "yes";
        public static readonly string BackupMail = "Hi [FirmName] this is system generated mail. Please check attachment for users backup.";
        public static readonly string BackupTxnMail = "Hi [FirmName] this is system generated mail. Please check attachment for all transactions.";

        public static readonly string BackupMailSubject = "Users Backup || " + DateTime.Now.ToString();
        public static readonly string BackupTxnMailSubject = "Transaction Backup || " + DateTime.Now.ToString();
        public static readonly string IsValidPan = "Invalid pan-card no";
        public static readonly string IsValidAadharNo = "Invalid aadhar no";
        public static readonly string ForgotPassLimitExceed = "Today's limit exceed for reset password, Please try another day.";

        public static readonly string BlockAccount = "Your account is deactivated, Please contact to customer care.";
        public static readonly string PasswordExpired = "Your password is expired, Please reset password.";
        public static string PasswordExpiry = "Your password will expire after [ReaminingDays] days.";

        public static readonly string OutsideSignUpDefaultValue = "yes";
        public static readonly string OutsideSignUpnotallowed = "Outside sign-up not allowed now, Please contact to customer care.";

        public static string NewUsergeneration = "FirmName: [FirmName]\n MobileNumber :[MobileNumber]";
        public static string KYCAcceptorReject = "Dear [FirmName], Your KYC is [Status] by [SystemName]";

        public static string TwoFAValidateSuccess = "2FA verified successfully";
        public static readonly string InvalidSecurityCode = "Please enter valid security code";

        public static readonly string InvoiceForNonGSTUserDefaultValue = "No";
        public static readonly string CheckPin = "CheckPin";

        public static string InvaildPanNumber = "Invaild PanNumber";
        public static string UnableToRevert = "Unable to revert this user";
        public static string UserTransferSuccess = "All users transfered successfully";
        public static string UserTransferFail = "Unable to transfer user";
        public static readonly string UserTransferSuccessFail = "[SuccessCount] users transfered successfully and [FailureCount] are unable to transfer.";

        public static readonly string InvalidCriteria = "Please select valid criteria for bulk update";

        public static readonly string BulkUserSuccessFail = "[SuccessCount] users updated successfully and [FailureCount] are unable to update.";

        public static readonly string InvalidPrimaryKeyId = "Invalid PrimaryKeyId";
        public static readonly string InvaildEncodeFPTxnId = "Invaild EncodeFPTxnId";
        public static readonly string InvaildTransactionID = "Invaild TransactionID";

        public static readonly string CommissionOnTransferOff = "Commission On Transfer is Off";
        public static readonly string OverdraftOff = "Overdraft is Off";

        public static readonly string AgentNotRegister = "Agent not registered";

        public static readonly string OTPsendSuccess = "OTP sent on your number";

        public static readonly string OTPverifiedsuccess = "OTP verified successfully";

        public static readonly string InvalidReferralOutsideSignUp = "Invalid referral user";

        public static readonly string OpenOnboarding = "OpenOnboarding";
        public static readonly string OpenOTP = "OpenOTP";
        public static readonly string Transaction = "Transaction";
        public static readonly string InvalidUserCreate = "Please purchase an ID and contact customer care.";
        public static readonly string AEPSDailyAuthentication = "Before proceeding, kindly initiate your AEPS daily authentication";
        public static readonly string AadharpayDailyAuthentication = "Before proceeding, kindly initiate your Aadharpay daily authentication";
        public static readonly string ReferralCheck = "Duplicate referral request";
        public static readonly string UserSuccess = "User validate successfully";


        public static readonly string BalanceEnquiry = "Balance enquiry done successfully";
        public static readonly string CashWithdraw = "Cash withdrawal done successfully";
        public static readonly string MiniStatement = "Mini statement successfull";
    }
}