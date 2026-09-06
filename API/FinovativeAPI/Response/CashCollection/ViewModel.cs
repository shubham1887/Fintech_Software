using Project.Processing;
using System;

namespace Project.Response.CashCollection
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private decimal _OutstandingOpening;

        public decimal OutstandingOpening
        {
            get => _OutstandingOpening;
            set => _OutstandingOpening = value;
        }

        private decimal _OutstandingClosing;

        public decimal OutstandingClosing
        {
            get => _OutstandingClosing;
            set => _OutstandingClosing = value;
        }

        private decimal _OutstandingAmount;

        public decimal OutstandingAmount
        {
            get => _OutstandingAmount;
            set => _OutstandingAmount = value;
        }

        private string _ParentRemark;

        public string ParentRemark
        {
            get => _ParentRemark.TrimIfNotNull();
            set => _ParentRemark = value.TrimIfNotNull();
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _Credit;

        public string Credit
        {
            get => _Credit.TrimIfNotNull();
            set => _Credit = value.TrimIfNotNull();
        }

        private string _Debit;

        public string Debit
        {
            get => _Debit.TrimIfNotNull();
            set => _Debit = value.TrimIfNotNull();
        }

        private string _UTRNumber;

        public string UTRNumber
        {
            get => _UTRNumber.TrimIfNotNull();
            set => _UTRNumber = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ParentFirmName;

        public string ParentFirmName
        {
            get => _ParentFirmName.TrimIfNotNull();
            set => _ParentFirmName = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private DateTime _OriginalDOC;

        public DateTime OriginalDOC
        {
            get => _OriginalDOC;
            set => _OriginalDOC = value;
        }
    }
}