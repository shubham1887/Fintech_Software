namespace Project.Message
{
    public class ProviderMessage
    {
        public static readonly string InValidProvider = "Invalid provider";
        public static readonly string InValidProviderDetails = "Invalid provider details";
        public static readonly string InValidProviderName = "Invalid provider name";
        public static readonly string InValidProviderURL = "Invalid provider url";
        public static readonly string InValidIP = "Invalid IP";
        public static readonly string ProviderNotFound = "Requested Provider not found";
        public static readonly string ProviderDeleteFail = "Provider not deleted";
        public static readonly string ProviderActiveDeactiveFail = "Provider not activedeactive";
        public static readonly string HLRonoffFail = "Unable to on off ";
        public static readonly string ProviderActiveDeactiveSuccess = "Sucessfully Provider activedeactive";
        public static readonly string ProviderUpdateSuccess = "Provider update successfully";
        public static readonly string ProviderUpdateFail = "Provider not updated";
        public static readonly string ProviderDeleteSuccess = "Provider deleted successfully.";
        public static readonly string ProviderDeailsDeleteSuccess = "Provider details deleted successfully.";
        public static readonly string InsertProviderSuccess = "Provider added successfully";
        public static readonly string InsertProviderDeailsSuccess = "Provider details submitted successfully";
        public static readonly string InsertProviderOperatorSuccess = "Provider Operator submitted successfully";
        public static readonly string InsertProviderFail = "Provider not added";
        public static readonly string InsertProviderOperatorFail = "Provider Operator not added";
        public static readonly string ActiveSuccess = "Provider activated successfully";
        public static readonly string DeactiveSuccess = "Provider deactivated successfully";
        public static readonly string HLRon = "HLR on successfully";
        public static readonly string HLRoff = "HLR off successfully";
        public static readonly string ProviderOperatorNotFound = "Provider operator not found";

        public static readonly string InValidConfigID = "Invalid ConfigutationID ";
        public static readonly string UrlNotFound = "Provider URL not found";
        public static readonly string StatusCheckUrlNotFound = "Status check URL not found";
        public static readonly string ComplaintUrlNotFound = "Complaint check URL not found";
        public static readonly string BalanceUrlNotFound = "Balance check URL not found";
        public static readonly string BillFetchUrlNotFound = "Bill fetch URL or sequence not found";
        public static readonly string BillFetchAPINotSet = "Bill fetch API not set";
        public static readonly string BalnceUpdateSuccess = "Balance update successfully";
        public static readonly string DuplicateDetails = "Duplicate details";

        public static readonly string ResponseNotFound = "Response Not found";
        public static readonly string SampleProviderNotFound = "Provider not found";
        public static readonly string ProviderInsertSuccess = "[SuccessCount] provider URL added successfully and [FailureCount] are already exists.";
        public static readonly string AlreadyExistsResponse = "[FailureCount] provider URL already exists.";
        public static readonly string Allredyset = "Please remove default or userwise sequence, then ON real api";
        public static readonly string RefreshSampleURL = "All sample URL refresh successfully.";
    }
}