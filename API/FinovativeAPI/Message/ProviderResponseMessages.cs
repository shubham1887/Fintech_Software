namespace Project.Message
{
    public class ProviderResponseMessage
    {
        public static readonly string InvalidProvider = "Please select valid provider";

        public static readonly string InvalidType = "Please select valid type";
        public static readonly string InvalidTextMustExists = "Please enter valid text must exists";
        public static readonly string InvalidResponse = "Please enter valid response";
        public static readonly string InvalidStatus = "Please select valid status";
        public static readonly string InsertSuccess = "Provider Response added successfully";
        public static readonly string UpdateFail = "Provider Response not updated";
        public static readonly string UpdateSuccess = "Provider Response  updated successfully";
        public static readonly string NotFound = "Requested provider response  not found";
        public static readonly string TextValid = "Please enter valid text";

        public static readonly string DeleteFail = "Provider Response not deleted";
        public static readonly string DeleteSuccess = "Provider Response deleted successfully";
        public static readonly string InvalidSampleResponse = "Please select valid sample response";
        public static readonly string InvalidDeveloper = "Please select valid developer";
        public static readonly string DuplicateResponse = "Provider Response already exists";
        public static readonly string UpdateFailForTextMustExists = "Response doesnt contain TextMustExists in it";

        public static readonly string ResponseInsertSuccess = "[SuccessCount] responses added Successfully and [FailureCount] are already exists.";
        public static readonly string AlreadyExistsResponse = "[FailureCount] responses already exists.";

        public static readonly string RefreshSampleResponse = "All sample reponse refresh successfully.";

        public static readonly string InvalidUser = "Please select valid User";
    }
}