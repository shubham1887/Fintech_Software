using Project.Processing;
using Project.StaticModel;

namespace Project.Response.Slab
{
    public class UserwiseSlabViewModel
    {
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _AmountString;

        public string AmountString
        {
            get => _AmountString.TrimIfNotNull();
            set => _AmountString = value.TrimIfNotNull();
        }

        private decimal _FromAmount;

        public decimal FromAmount
        {
            get => _FromAmount;
            set => _FromAmount = value;
        }

        private decimal _ToAmount;

        public decimal ToAmount
        {
            get => _ToAmount;
            set => _ToAmount = value;
        }

        private decimal _RetailerComm;

        public decimal RetailerComm
        {
            get => _RetailerComm;
            set => _RetailerComm = value;
        }

        private string _RetailerCommType;

        public string RetailerCommType
        {
            get => _RetailerCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RetailerCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RetailerCommissionCategory;

        public string RetailerCommissionCategory
        {
            get => _RetailerCommissionCategory.TrimIfNotNull();
            set => _RetailerCommissionCategory = value.TrimIfNotNull();
        }
    }
}