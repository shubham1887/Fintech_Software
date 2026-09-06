using Project.Processing;
using System;

namespace Project.Response.Operator
{
    public class ViewModel
    {
        public int ServiceID
        {
            get; set;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _Path;

        public string Path
        {
            get => _Path.TrimIfNotNull();
            set => _Path = value.TrimIfNotNull();
        }

        private string _Opcode;

        public string Opcode
        {
            get => _Opcode;
            set => _Opcode = value;
        }

        private decimal _MinimumRechargeAmount;

        public decimal MinimumRechargeAmount
        {
            get => _MinimumRechargeAmount;
            set => _MinimumRechargeAmount = value;
        }

        private decimal _MaximumRechargeAmount;

        public decimal MaximumRechargeAmount
        {
            get => _MaximumRechargeAmount;
            set => _MaximumRechargeAmount = value;
        }

        private int _RepeatRechargeInMinute;

        public int RepeatRechargeInMinute
        {
            get => _RepeatRechargeInMinute;
            set => _RepeatRechargeInMinute = value;
        }

        private int _BlockedTill;

        public int BlockedTill
        {
            get => _BlockedTill;
            set => _BlockedTill = value;
        }

        private bool _IsTimelyBlocked;

        public bool IsTimelyBlocked
        {
            get => _IsTimelyBlocked;
            set => _IsTimelyBlocked = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _BusinessModel;

        public string BusinessModel
        {
            get => _BusinessModel.TrimIfNotNull();
            set => _BusinessModel = value.TrimIfNotNull();
        }

        private string _HSNCode;

        public string HSNCode
        {
            get => _HSNCode.TrimIfNotNull();
            set => _HSNCode = value.TrimIfNotNull();
        }

        private string _SequenceLogic;

        public string SequenceLogic
        {
            get => _SequenceLogic.TrimIfNotNull();
            set => _SequenceLogic = value.TrimIfNotNull();
        }

        private string _AccountStartWith;

        public string AccountStartWith
        {
            get => _AccountStartWith.TrimIfNotNull();
            set => _AccountStartWith = value.TrimIfNotNull();
        }
        
        private bool _IsViewRequired;
        public bool IsViewRequired

        {
            get => _IsViewRequired;
            set => _IsViewRequired = value;
        }

        private bool _IsBBPS;

        public bool IsBBPS

        {
            get => _IsBBPS;
            set => _IsBBPS = value;
        }

        private bool _IsBillFetch;

        public bool IsBillFetch
        {
            get { return _IsBillFetch; }
            set { _IsBillFetch = value; }
        }

        private bool _IsPartialBill;

        public bool IsPartialBill
        {
            get { return _IsPartialBill; }
            set { _IsPartialBill = value; }
        }

        private int _MaxLength;

        public int MaxLength
        {
            get { return _MaxLength; }
            set { _MaxLength = value; }
        }

        private int _MinLength;

        public int MinLength
        {
            get { return _MinLength; }
            set { _MinLength = value; }
        }

        private string _TollFreeNumber;

        public string TollFreeNumber
        {
            get { return _TollFreeNumber.TrimIfNotNull(); }
            set { _TollFreeNumber = value.TrimIfNotNull(); }
        }

        private string _AccountName;

        public string AccountName
        {
            get { return _AccountName.TrimIfNotNull(); }
            set { _AccountName = value.TrimIfNotNull(); }
        }

        private string _Remark;

        public string Remark
        {
            get { return _Remark.TrimIfNotNull(); }
            set { _Remark = value.TrimIfNotNull(); }
        }

        public string OperatorDetail
        {
            get => $" Service : {ServiceName}{Environment.NewLine}{OperatorName}";
        }

        public string AmountDetail
        {
            get => $"{Convert.ToInt32(MinimumRechargeAmount)} to {Convert.ToInt32(MaximumRechargeAmount)}";
        }

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _MaxAmount;

        public decimal MaxAmount
        {
            get => _MaxAmount;
            set => _MaxAmount = value;
        }

        private decimal _UISequence;

        public decimal UISequence
        {
            get => _UISequence;
            set => _UISequence = value;
        }

        private string _AllowedFor;

        public string AllowedFor
        {
            get => _AllowedFor.TrimIfNotNull();
            set => _AllowedFor = value.TrimIfNotNull();
        }

        private string _BlockedAmount;

        public string BlockedAmount
        {
            get => _BlockedAmount.TrimIfNotNull();
            set => _BlockedAmount = value.TrimIfNotNull();
        }
    }
}