using Project.Processing;

namespace Project.Response.InvoiceSetting
{
    public class ViewModel
    {
        private int _InvoiceSettingID;

        public int InvoiceSettingID
        {
            get => _InvoiceSettingID;
            set => _InvoiceSettingID = value;
        }

        private string _LogoURL;

        public string LogoURL
        {
            get => _LogoURL.TrimIfNotNull();
            set => _LogoURL = value.TrimIfNotNull();
        }

        private string _SignatureURL;

        public string SignatureURL
        {
            get => _SignatureURL.TrimIfNotNull();
            set => _SignatureURL = value.TrimIfNotNull();
        }

        private string _CompanyPrefix;

        public string CompanyPrefix
        {
            get => _CompanyPrefix.TrimIfNotNull();
            set => _CompanyPrefix = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _GSTIN;

        public string GSTIN
        {
            get => _GSTIN.TrimIfNotNull();
            set => _GSTIN = value.TrimIfNotNull();
        }

        private string _PAN;

        public string PAN
        {
            get => _PAN.TrimIfNotNull();
            set => _PAN = value.TrimIfNotNull();
        }

        private string _CIN;

        public string CIN
        {
            get => _CIN.TrimIfNotNull();
            set => _CIN = value.TrimIfNotNull();
        }

        private string _District;

        public string District
        {
            get => _District.TrimIfNotNull();
            set => _District = value.TrimIfNotNull();
        }

        private string _Country;

        public string Country
        {
            get => _Country.TrimIfNotNull();
            set => _Country = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _AccountType;

        public string AccountType
        {
            get => _AccountType.TrimIfNotNull();
            set => _AccountType = value.TrimIfNotNull();
        }

        private string _AccountHolderName;

        public string AccountHolderName
        {
            get => _AccountHolderName.TrimIfNotNull();
            set => _AccountHolderName = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _BranchName;

        public string BranchName
        {
            get => _BranchName.TrimIfNotNull();
            set => _BranchName = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _WebsiteName;

        public string WebsiteName
        {
            get => _WebsiteName.TrimIfNotNull();
            set => _WebsiteName = value.TrimIfNotNull();
        }

        private string _Note;

        public string Note
        {
            get => _Note.TrimIfNotNull();
            set => _Note = value.TrimIfNotNull();
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private string _FullAddress;

        public string FullAddress
        {
            get => _FullAddress.TrimIfNotNull();
            set => _FullAddress = value.TrimIfNotNull();
        }

        private string _ShortAddress;

        public string ShortAddress
        {
            get => _ShortAddress.TrimIfNotNull();
            set => _ShortAddress = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }
    }
}