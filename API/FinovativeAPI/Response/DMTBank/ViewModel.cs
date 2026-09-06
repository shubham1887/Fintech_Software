using Project.Processing;

namespace Project.Response.DMTBank
{
    public class ViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private int _BankID;

        public int BankID
        {
            get => _BankID;
            set => _BankID = value;
        }

        private string _DefaultIFSC;

        public string DefaultIFSC
        {
            get => _DefaultIFSC.TrimIfNotNull();
            set => _DefaultIFSC = value.TrimIfNotNull();
        }

        private bool _IsIMPS;

        public bool IsIMPS
        {
            get => _IsIMPS;
            set => _IsIMPS = value;
        }
    }
}