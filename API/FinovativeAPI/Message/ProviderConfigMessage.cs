namespace Project.Message
{
    public class ProviderConfigMessage
    {
        public static readonly string UpdateSuccess = "Provider Operator update successfully";
        public static readonly string InvalidCode = "Invalid code";
        public static readonly string UpdateFailure = "Unable to update provider operator";
        public static readonly string Datanotfound = "Record not found";
        public static readonly string InvalidConfigurationID = "Invalid ConfigurationID";
        public static readonly string OperatorOff = "Provider operator is off";
        public static readonly string OpcodeNotfound = "Opcode not found";
        public static readonly string InvalidCircle = "Invalid circle";
        public static readonly string CircleMappingInsertSuccess = "Circle mapping added successfully";
        public static readonly string DuplicateResponse = "Provider configuration already exists";
        public static readonly string ConfigurationInsertSuccess = "[SuccessCount] operator configuration added Successfully and [FailureCount] are already exists.";
        public static readonly string AlreadyExistsConfiguration = "[FailureCount] operator configuration already exists.";
        public static readonly string InvalidProvider = "Please select valid provider";
        public static readonly string InvalidSampleConfiguration = "Please select valid configuration";
        public static readonly string RefreshSampleConfiguration = "All sample configuration refresh successfully.";
        public static readonly string StatecodeUpdateFailure = "Unable to update Statecode";
        public static readonly string StatecodeUpdateSuccess = "Statecode update successfully";
    }
}