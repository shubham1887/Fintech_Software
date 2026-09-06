namespace Project.Message
{
    public class OperatorStateMappingMessage
    {
        public static readonly string InsertFailure = "Operator State not added";
        public static readonly string InsertSuccess = "Operator State added successfully";
        public static readonly string DeleteSuccess = "Operator State deleted successfully";
        public static readonly string DeleteFail = "Operator State not deleted ";
        public static readonly string UpdateFail = "Operator State not updated";
        public static readonly string UpdateSuccess = "Operator State updated successfully";
        public static readonly string InvalidHLR = "Invalid HLR";
        public static readonly string InvalidOperator = "Invalid operator";
        public static readonly string InvalidState = "Invalid state";
        public static readonly string NoRecord = "Record no found";
        public static readonly string ActiveSuccess = "Operator state activated successfully";
        public static readonly string DeactiveSuccess = "Operator state deactivated successfully";
        public static readonly string InValidID = "Invalid ID";
    }
}