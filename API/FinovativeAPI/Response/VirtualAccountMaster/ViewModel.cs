using Project.Processing;

namespace Project.Response.VirtualAccountMaster
{
    public class ViewModel
    {
        private int _VirtualAccountMasterID;

        public int VirtualAccountMasterID
        {
            get => _VirtualAccountMasterID;
            set => _VirtualAccountMasterID = value;
        }

        private string _Prefix;

        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
    }
}