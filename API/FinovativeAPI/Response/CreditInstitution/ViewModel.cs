using Project.Processing;

namespace Project.Response.CreditInstitution
{
    public class ViewModel
    {
        private int _CreditInstitutionMasterID;

        public int CreditInstitutionMasterID
        {
            get => _CreditInstitutionMasterID;
            set => _CreditInstitutionMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsActiveWeb;

        public bool IsActiveWeb
        {
            get => _IsActiveWeb;
            set => _IsActiveWeb = value;
        }

        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _EncryptionKey;

        public string EncryptionKey
        {
            get => _EncryptionKey.TrimIfNotNull();
            set => _EncryptionKey = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _BranchID;

        public string BranchID
        {
            get => _BranchID.TrimIfNotNull();
            set => _BranchID = value.TrimIfNotNull();
        }

    }
}