using Project.Processing;

namespace Project.Response.OfflineProductMaster
{
    public class ViewModel
    {
        private int _OfflineProductMasterID;

        public int OfflineProductMasterID
        {
            get => _OfflineProductMasterID;
            set => _OfflineProductMasterID = value;
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

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private decimal _Cost;

        public decimal Cost
        {
            get => _Cost;
            set => _Cost = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Validity;

        public string Validity
        {
            get => _Validity;
            set => _Validity = value;
        }

        private string _OpType;

        public string OpType
        {
            get => _OpType.TrimIfNotNull();
            set => _OpType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private decimal _PackageMRP;

        public decimal PackageMRP
        {
            get => _PackageMRP;
            set => _PackageMRP = value;
        }

        private decimal _BookingAmount;

        public decimal BookingAmount
        {
            get => _BookingAmount;
            set => _BookingAmount = value;
        }
    }
}