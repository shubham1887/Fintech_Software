namespace Project.Message
{
    public class TransactionMessage
    {
        public static readonly string DuplicateInbox = "Duplicate transaction, please wait for minute!";
        public static readonly string DuplicateTransaction = "Duplicate transaction";
        
        public static readonly string InvalidAmount = "Please enter amount";
        public static readonly string InvalidAccountNumber = "Please enter valid accountnumber";
        public static readonly string AdminBalanceLow = "Please try after some time admin balance low ";
        public static readonly string NotpermittedAPI = "Shift is not permitted to a pending API, please select another API";

        public static readonly string InvalidRechargePin = "Please enter valid recharge pin";
        public static readonly string InvalidProviderID = "Invalid provider code, please contact to customer care";
        public static readonly string InvalidAuthCode = "Invalid auth code, please contact to customer care";
        public static readonly string BacklistAadhar = "This aadhar number is on a backlist";

        public static readonly string TransactionNotYetSend = "Transaction not yet send, wait for some time";
        public static readonly string ProcessMessage = "Transaction is in process";
        public static readonly string TransactionNotFound = "Transaction not found";
        public static readonly string NotFound = "NotFound";

        public static readonly string ServiceChargeDeductionFail = "Service charge is not deducted. Your service charge is [Charge]";

        public static readonly string UDD = "[AlleUserID]#[UniqueRefNo]#[MobileNumber]";

        public static readonly string ServiceIsOff = "Service is deactivated for you, please contact to customer care";
        public static readonly string ServiceIsOffForParent = "Service is deactivated for your parent, please contact to your parent";
        public static readonly string ServiceIsOffForSuperparent = "Service is deactivated for your super parent, please contact to your super parent";

        public static readonly string BlockedAmount = "Please provide valid amount";
        public static readonly string BlockedUserWiseAmount = "Please provide valid amount";

        public static readonly string OperatorOff = "Operator temporary not available";

        public static readonly string InValidRechargeAmount = "Invalid amount";
        public static readonly string InvalidMarginType = "Invalid Margin Type";

        public static readonly string InvalidBankID = "Invalid BankID";

        public static string Failure = "Fail: unable to do transaction";
        public static string UpdateManual = "Manually status updated";
        public static string AllreadyFail = "Already fail";
        public static string NotInProcess = "Transaction not in process";
        public static string TransactionInProcess = "Transaction in process";
        public static string AllreadySucess = "Already success";
        public static string UpdateFail = "Unable to update";
        public static string TransSucessDescription = "Transaction successful-manual";
        public static string TransFailDescription = "Transaction Fail-manual";
        public static string TransFailByComplaintDescription = "Transaction Fail- by complaint";
        public static readonly string InvalidDate = "Invalid date";
        public static readonly string InvalidAEPSAmount = "Invalid amount, should greater than 50";
        public static readonly string InvalidAEPSAmountInBetween = "Invalid amount, should between [From] and [To]";
        public static readonly string InvalidAEPSServiceCode = "Invalid AEPS Service Code/Operator code";
        public static readonly string InvalidMATMServiceCode = "Invalid M-ATM Service Code/Operator code";
        public static readonly string InvalidPOSServiceCode = "Invalid POS Service Code/Operator code";
        public static readonly string InvalidCMSServiceCode = "Invalid CMS Service Code";
        public static readonly string InvalidPOSOperarorCode = "Invalid POS Operator code";
        public static readonly string InvalidCMSOperarorCode = "Invalid CMS Operator code";

        public static readonly string InvalidBusBookingOperarorCode = "Invalid BusBooking Operator code";
        public static readonly string InvalidBusBookingServiceCode = "Invalid BusBooking Service Code";

        public static readonly string InvalidDMTServiceCode = "Invalid DMT Service Code/Operator code";

        public static readonly string Invalidamount = "Invalid amount";
        public static readonly string InvalidUser = "Invalid user";
        public static readonly string InvalidAadhar = "Please set your Aadhar no in profile ";
        public static readonly string InvalidPAN = "Please set your PAN in profile ";
        public static readonly string NotUpdateStatus = "Status can not update for AEPS transaction";

        public static readonly string InvalidConsumerNumber = "Invalid consumer number";
        public static readonly string InvalidStatus = "Invalid status";

        public static readonly string InvalidRequestID = "Invalid request id";

        public static readonly string InvalidID = "Invalid id";
        public static readonly string InvalidType = "Invalid type";
        public static readonly string ProviderUpdateFailure = "Unable to update Provider ";
        public static readonly string NotFoundID = "Not found id";
        public static readonly string RecordNotFound = "Record not found";

        public static readonly string NCPINewRuleForAEPSTransaction = "Failed due to NPCI eFRM rule, Please do transaction after 30 minutes";
        public static readonly string DailyAEPSFailexceeds = "The daily Aadhar number failure limit has been exceeded";

        public static readonly string ProviderSequenceNotFound = "Requested Provider Sequence not found";
        public static readonly string TransactionURLNotFound = "Transaction URL not set, Please contact to customer care";
        public static readonly string FormatURLNotFound = "URL formatting not found";
        public static readonly string Opcodenotfound = "Opcode not found";
        public static readonly string sequenceNotFound = "sequence not found, please contact to customer care";
        public static readonly string StopDenomination = "sequence not found due to stop denomination, Please contact to customer care";
        public static readonly string PendingCount = "sequence not found due to operator pending validation, Please contact to customer care";
        public static readonly string FailCount = "sequence not found due to operator fail validation, Please contact to customer care";
        public static readonly string sequenceNotFoundOrCircle = "sequence not found or circle wise api not set, please contact to customer care";

        public static readonly string NEFTServiceNotAvailable = "NEFT service not available, please contact to customer care";

        public static readonly string ProviderLowBalance = "Service is down, error code LB, please contact to customer care";

        public static readonly string RechargeIsProcessedAlready = "Recharge processed already";
        public static readonly string Callback = "Callback";
        public static readonly string Failuremessage = "Recharge Failed";
        public static readonly string Successmessage = "Recharge done successfully";

        public static readonly string SuccessUpdate = "Recharge update successfully";
        public static readonly string BlankResponse = "Blank response";
        public static readonly string PenidngProviderResponse = "Penidng provider request";
        public static readonly string ValidTransectionID = "Provide valid transaction id";
        public static readonly string ResendSuccess = "Recharge resend successfully";

        public static readonly string RefundAllSuccess = "Refund all completed";
        public static readonly string StatusCheckAllSuccess = "Status check all completed";
        public static readonly string SuccessAllSuccess = "Success all completed";
        public static readonly string TransactionShouldOnlyMobile = "Transaction should only mobile service";

        public static readonly string ShiftSuccess = "Recharge shift successfully";
        public static readonly string BlankResponseFromStatusCheckAPI = "Blank response from status check API";

        public static readonly string InvalidTransaction = "Please select valid transaction";

        public static readonly string FCM_Message_Mobile_Transaction_Pending = " Transaction: [Status] for [Operator] with [ConsumerNumber] and [Amount]";

        public static readonly string FCM_Message_DMT_Transaction_Pending = "Money Transfer [Status] of [Amount] for [AccountNumber]";
        public static readonly string InvalidUserToUpdateTransaction = "Before 30 days transactions are not allowed to update";
        public static readonly string InvalidRemark = "Invalid remark";
        public static readonly string RechargeAccepted = "Recharge request accepted successfully";

        public static readonly string InvalidMobileNumber = "Please enter valid consumer number";

        public static readonly string ValidBeneficiaryVerified = "Beneficiary is already verified";

        public static readonly string InvalidCardNumber = "Please enter valid card number";

        public static readonly string CVVNumber = "Please enter valid cvv number";

        public static readonly string InvalidCardNetwork = "Please select valid card network";

        public static readonly string InvalidOTPRefernce = "Invalid OTPRefernce";
        public static readonly string InvalidOTP = "Invalid OTP";

        public static readonly string InvalidName = "Please enter valid card holder name";
        public static readonly string InvalidPayeeName = "Please enter valid payee name";
        
        public static readonly string InvalidUPIIDName = "Please enter valid name";
        public static readonly string InvalidMicroATMName = "Invalid microATM name";
        public static readonly string InvalidUPIID = "Invalid UPIID";
        public static readonly string InvalidUPIIDLength = "Invalid UPIID length";

        public static readonly string PlanViewLimitOver = "Your free hit is over,Do one transaction";
        public static readonly string PlanViewStatus = "Plan/circle fetch API is off for you, Please contact to Admin";
        public static readonly string BillViewStatus = "Bill fetch API is off for you, Please contact to Admin";
        public static readonly string PlanNotFound = "Plan Not Found";
        public static readonly string InvalidHeader = "Invalid header data";
        public static readonly string SettingNotFound = "Setting Not Available";
        public static readonly string DTHNotFound = "DTH Info Not Found";
    }
}