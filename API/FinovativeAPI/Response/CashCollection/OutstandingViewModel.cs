using Project.Processing;

namespace Project.Response.CashCollection
{
    public class OutstandingViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }
        private decimal _Outstanding;

        public decimal Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }
    }
}
