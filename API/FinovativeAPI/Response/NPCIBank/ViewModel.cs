using Project.Processing;

namespace Project.Response.NPCIBank
{
    public class ViewModel
    {
        private string _BankName;

        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _IIN;

        public string IIN
        {
            get => _IIN;
            set => _IIN = value;
        }
    }
}
