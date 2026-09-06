using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    public class SenderSelectResponse
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

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _LastName;

        public string LastName
        {
            get => _LastName.TrimIfNotNull();
            set => _LastName = value.TrimIfNotNull();
        }

        private string _RemainingLimit;

        public string RemainingLimit
        {
            get => _RemainingLimit;
            set => _RemainingLimit = value;
        }

        private string _ComsumedLimit;

        public string ComsumedLimit
        {
            get => _ComsumedLimit;
            set => _ComsumedLimit = value;
        }
    }
}
