using Project.Processing;

namespace Project.Response.UserBalanceCapping
{
    public class CappingViewModel
    {
        public int WalletID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ShortName;

        public string ShortName
        {
            get => _ShortName.TrimIfNotNull();
            set => _ShortName = value.TrimIfNotNull();
        }
    }
}