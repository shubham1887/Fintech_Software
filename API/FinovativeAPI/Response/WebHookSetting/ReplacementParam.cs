using Project.Processing;

namespace Project.Response.WebHookSetting
{
    public class ReplacementParam
    {
        private string _BaseURL;

        public string BaseURL
        {
            get => _BaseURL.TrimIfNotNull();
            set => _BaseURL = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private string _ParentNo;

        public string ParentNo
        {
            get => _ParentNo.TrimIfNotNull();
            set => _ParentNo = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _SystemName;

        public string SystemName
        {
            get => _SystemName.TrimIfNotNull();
            set => _SystemName = value.TrimIfNotNull();
        }

        private string _Receiver;

        public string Receiver
        {
            get => _Receiver.TrimIfNotNull();
            set => _Receiver = value.TrimIfNotNull();
        }

        private string _ReceiverMobileNumber;

        public string ReceiverMobileNumber
        {
            get => _ReceiverMobileNumber.TrimIfNotNull();
            set => _ReceiverMobileNumber = value.TrimIfNotNull();
        }

        private string _Sender;

        public string Sender
        {
            get => _Sender.TrimIfNotNull();
            set => _Sender = value.TrimIfNotNull();
        }

        private string _SenderMobileNumber;

        public string SenderMobileNumber
        {
            get => _SenderMobileNumber.TrimIfNotNull();
            set => _SenderMobileNumber = value.TrimIfNotNull();
        }

        private string _Wallet;

        public string Wallet
        {
            get => _Wallet.TrimIfNotNull();
            set => _Wallet = value.TrimIfNotNull();
        }

        private string _ToWallet;

        public string ToWallet
        {
            get => _ToWallet.TrimIfNotNull();
            set => _ToWallet = value.TrimIfNotNull();
        }

        private string _OldBalance;

        public string OldBalance
        {
            get => _OldBalance.TrimIfNotNull();
            set => _OldBalance = value.TrimIfNotNull();
        }

        private string _CurrentBalance;

        public string CurrentBalance
        {
            get => _CurrentBalance.TrimIfNotNull();
            set => _CurrentBalance = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _TxnID;

        public string TxnID
        {
            get => _TxnID.TrimIfNotNull();
            set => _TxnID = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Outstanding;

        public string Outstanding
        {
            get => _Outstanding.TrimIfNotNull();
            set => _Outstanding = value.TrimIfNotNull();
        }

        private string _OldOutstanding;

        public string OldOutstanding
        {
            get => _OldOutstanding.TrimIfNotNull();
            set => _OldOutstanding = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _UTRNumber;

        public string UTRNumber
        {
            get => _UTRNumber.TrimIfNotNull();
            set => _UTRNumber = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _Provider;

        public string Provider
        {
            get => _Provider.TrimIfNotNull();
            set => _Provider = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _PANNo;

        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }

        private string _EmailId;

        public string EmailId
        {
            get => _EmailId.TrimIfNotNull();
            set => _EmailId = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }
    }
}