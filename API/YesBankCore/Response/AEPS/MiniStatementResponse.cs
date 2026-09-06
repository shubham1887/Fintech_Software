using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    public class MiniStatementResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _BankRRN;

        public string BankRRN
        {
            get => _BankRRN.TrimIfNotNull();
            set => _BankRRN = value.TrimIfNotNull();
        }

        private string _MiniStatement;

        public string MiniStatement
        {
            get => _MiniStatement.TrimIfNotNull();
            set => _MiniStatement = value.TrimIfNotNull();
        }

        private string _DateTime;

        public string DateTime
        {
            get => _DateTime.TrimIfNotNull();
            set => _DateTime = value.TrimIfNotNull();
        }
        private string _Balance;

        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }

        private string _DeviceType;

        public string DeviceType
        {
            get => _DeviceType.TrimIfNotNull();
            set => _DeviceType = value.TrimIfNotNull();
        }
        private string _DeviceModel;

        public string DeviceModel
        {
            get => _DeviceModel.TrimIfNotNull();
            set => _DeviceModel = value.TrimIfNotNull();
        }
        private string _SysTraceNo;

        public string SysTraceNo
        {
            get => _SysTraceNo.TrimIfNotNull();
            set => _SysTraceNo = value.TrimIfNotNull();
        }
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        private string _AgentID;

        public string AgentID
        {
            get => _AgentID.TrimIfNotNull();
            set => _AgentID = value.TrimIfNotNull();
        }
    }
}
