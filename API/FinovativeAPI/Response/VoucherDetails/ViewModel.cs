using Project.Processing;
using System;

namespace Project.Response.VoucherDetails
{
    public class ViewModel
    {


        private int _VoucherDetailsID;

        public int VoucherDetailsID
        {
            get => _VoucherDetailsID;
            set => _VoucherDetailsID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }
        private decimal _OutstandingOpeningBal;

        public decimal OutstandingOpeningBal
        {
            get => _OutstandingOpeningBal;
            set => _OutstandingOpeningBal = value;
        }
        

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private decimal _OutstandingClossingBal;

        public decimal OutstandingClossingBal
        {
            get => _OutstandingClossingBal;
            set => _OutstandingClossingBal = value;
        }
        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
        private int _DocInt;

        public int DocInt
        {
            get => _DocInt;
            set => _DocInt = value;
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
      
    }
}
