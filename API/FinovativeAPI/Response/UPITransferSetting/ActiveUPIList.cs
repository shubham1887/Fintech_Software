using Project.Processing;

namespace Project.Response.UPITransferSetting
{
    public class ActiveUPIList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNullWithDefault("UPITransfer");
            set => _DisplayName = value.TrimIfNotNullWithDefault("UPITransfer");
        }

        private bool _IsCommonUPI;

        public bool IsCommonUPI
        {
            get => _IsCommonUPI;
            set => _IsCommonUPI = value;
        }

        private bool _IsUPITransferWithSender;

        public bool IsUPITransferWithSender
        {
            get => _IsUPITransferWithSender;
            set => _IsUPITransferWithSender = value;
        }
    }
}