using Project.Processing;

namespace Project.Response.CallMeRequest
{
    public class ViewModel
    {
        private int _CallMeRequestID;

        public int CallMeRequestID
        {
            get => _CallMeRequestID;
            set => _CallMeRequestID = value;
        }

        private int _CallMeCategoryMasterID;

        public int CallMeCategoryMasterID
        {
            get => _CallMeCategoryMasterID;
            set => _CallMeCategoryMasterID = value;
        }

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

        private int _UplineUserID;

        public int UplineUserID
        {
            get => _UplineUserID;
            set => _UplineUserID = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _UplineRemark;

        public string UplineRemark
        {
            get => _UplineRemark.TrimIfNotNull();
            set => _UplineRemark = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _UplineName;

        public string UplineName
        {
            get => _UplineName;
            set => _UplineName = value;
        }

        private int _DOCInt;
        public int DOCInt { get => _DOCInt; set => _DOCInt = value; }
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
    }
}