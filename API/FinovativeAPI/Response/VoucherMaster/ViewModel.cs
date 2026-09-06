using Project.Processing;
using System;

namespace Project.Response.VoucherMaster
{
    public class ViewModel
    {


        private int _VoucherMasterID;

        public int VoucherMasterID
        {
            get => _VoucherMasterID;
            set => _VoucherMasterID = value;
        }

        private int _VoucherTypeID;

        public int VoucherTypeID
        {
            get => _VoucherTypeID;
            set => _VoucherTypeID = value;
        }

        private string _VoucherDate;

        public string VoucherDate
        {
            get => _VoucherDate.TrimIfNotNull();
            set => _VoucherDate = value.TrimIfNotNull();
        }

        private string _VoucherType;

        public string VoucherType
        {
            get => _VoucherType.TrimIfNotNull();
            set => _VoucherType = value.TrimIfNotNull();
        }
        private string _CrGroup;

        public string CrGroup
        {
            get => _CrGroup.TrimIfNotNull();
            set => _CrGroup = value.TrimIfNotNull();
        }
        private string _CrName;

        public string CrName
        {
            get => _CrName.TrimIfNotNull();
            set => _CrName = value.TrimIfNotNull();
        }
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private string _DrGroup;

        public string DrGroup
        {
            get => _DrGroup.TrimIfNotNull();
            set => _DrGroup = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        private string _DrName;

        public string DrName
        {
            get => _DrName.TrimIfNotNull();
            set => _DrName = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        public int DOCInt { get; internal set; }
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }
       

        
        private string _SubmitedByName;

        public string SubmitedByName
        {
            get => _SubmitedByName.TrimIfNotNull();
            set => _SubmitedByName = value.TrimIfNotNull();
        }
        private int _SubmitedBy;

        public int SubmitedBy
        {
            get => _SubmitedBy;
            set => _SubmitedBy = value;
        }
        private string _ApproveByName;

        public string ApproveByName
        {
            get => _ApproveByName.TrimIfNotNull();
            set => _ApproveByName = value.TrimIfNotNull();
        }
        private int _ApproveBy;

        public int ApproveBy
        {
            get => _ApproveBy;
            set => _ApproveBy = value;
        }
        public bool Is_Delete { get; set; }

    }
}
