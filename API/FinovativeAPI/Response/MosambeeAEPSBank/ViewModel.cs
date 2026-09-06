using Project.Processing;

namespace Project.Response.MosambeeAEPSBank
{
    public class ViewModel
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _IIN;

        public string IIN
        {
            get => _IIN.TrimIfNotNull();
            set => _IIN = value.TrimIfNotNull();
        }
    }
}