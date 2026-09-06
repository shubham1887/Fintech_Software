using Project.Processing;


namespace Project.Response.BlackListBank
{
    public class ViewModel
    {
        private int _BlackListBankMasterID;

        public int BlackListBankMasterID
        {
            get => _BlackListBankMasterID;
            set => _BlackListBankMasterID = value;
        }
        private string _BankName;

        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
    }
}
