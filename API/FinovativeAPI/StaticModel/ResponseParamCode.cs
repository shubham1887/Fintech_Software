namespace Project.StaticModel
{
    public class ResponseParamCode
    {
        // [RequestID] [Amount] [Number] [TransID] [Message] [ClBal] [DMTBalance] [LapuNo]

        public static readonly string RequestID = "[RequestID]";

        public static readonly string Amount = "[Amount]";
        public static readonly string Number = "[Number]";
        public static readonly string TxnID = "[TxnID]";
        public static readonly string Message = "[Message]";
        public static readonly string ClBal = "[ClBal]";

        public static readonly string DMTBalance = "[DMTBalance]";
        public static readonly string LapuNo = "[LapuNo]";

        public static readonly string RefID = "[RefID]";
        public static readonly string Date = "[Date]";

        public static readonly string CustomerName = "[CustomerName]";

        public static readonly string DueDate = "[DueDate]";
        public static readonly string ApiComm = "[ApiComm]";
        public static readonly string BBPSViewID = "[BBPSViewID]";
        public static readonly string ROffer = "[ROffer]";
        public static readonly string ApiCommPer = "[ApiCommPer]";

        // added for view parameter required in bbps transaction
        public static readonly string ResponseParam1 = "[ResponseParam1]";

        public static readonly string ResponseParam2 = "[ResponseParam2]";
        public static readonly string ResponseParam3 = "[ResponseParam3]";
        public static readonly string ResponseParam4 = "[ResponseParam4]";
        public static readonly string ResponseParam5 = "[ResponseParam5]";
    }
}