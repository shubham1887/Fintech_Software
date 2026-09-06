using Project.Processing;

namespace Project.Response.UserCreationSetting
{
    public class ViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _MinimumCount;

        public int MinimumCount
        {
            get => _MinimumCount;
            set => _MinimumCount = value;
        }

        private decimal _Cost;

        public decimal Cost
        {
            get => _Cost;
            set => _Cost = value;
        }

        private int _DOCInt;

        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int _UserCreationSettingID;

        public int UserCreationSettingID
        {
            get => _UserCreationSettingID;
            set => _UserCreationSettingID = value;
        }
    }
}