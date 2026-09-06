using Project.Processing;

namespace Project.Response.PaySprintBank
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
    }
}