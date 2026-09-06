using Project.Processing;

namespace Project.Response.DMTBank
{
    public class BankViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private int _DMTBankID;

        public int DMTBankID
        {
            get => _DMTBankID;
            set => _DMTBankID = value;
        }

        private string _DefaultIFSC;

        public string DefaultIFSC
        {
            get => _DefaultIFSC.TrimIfNotNull();
            set => _DefaultIFSC = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsIMPS;

        public bool IsIMPS
        {
            get => _IsIMPS;
            set => _IsIMPS = value;
        }
    }
}