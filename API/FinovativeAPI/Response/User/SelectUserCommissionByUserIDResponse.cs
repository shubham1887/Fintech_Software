using Project.Processing;

namespace Project.Response.User
{
    public class SelectUserCommissionByUserIDResponse
    {
        private decimal _Comm;

        public decimal Comm
        {
            get => _Comm;
            set => _Comm = value;
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);

            set => _CommissionCategory = value.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
        }

        private string _CommType;

        public string CommType
        {
            get => _CommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);

            set => _CommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
        }
    }
}