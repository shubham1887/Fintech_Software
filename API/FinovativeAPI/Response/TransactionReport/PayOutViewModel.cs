using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class PayOutViewModel
    {
        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
    }
}