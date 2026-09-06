using MplanCore.Controller;
using System;

namespace Project.Response.AEPS
{
    public class ViewModel
    {
        private int _AuthenticationID;

        public int AuthenticationID
        {
            get => _AuthenticationID;
            set => _AuthenticationID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC;
            set => _DOC = value;
        }
        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }
        private int _OperatorID;
        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }
        private string _Pipe;
        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }
        private string _Operator;
        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }
        private string _ProviderName;
        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _Status;
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        
        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private DateTime _DOCDateTime;
        public DateTime DOCDateTime
        {
            get => _DOCDateTime;
            set => _DOCDateTime = value;
        }

        private bool _Is2FA;
        public bool Is2FA
        {
            get => _Is2FA;
            set => _Is2FA = value;
        }
        private bool _IsMerchantVerify;
        public bool IsMerchantVerify
        {
            get => _IsMerchantVerify;
            set => _IsMerchantVerify = value;
        }
    }
}