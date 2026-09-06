using Project.Processing;

namespace Project.Response.SupportUserAccessDetail
{
    public class ViewModel
    {
        private int _SupportUserAccessDetailID;

        public int SupportUserAccessDetailID
        {
            get => _SupportUserAccessDetailID;
            set => _SupportUserAccessDetailID = value;
        }

        private int _SupportUserAccessMasterID;

        public int SupportUserAccessMasterID
        {
            get => _SupportUserAccessMasterID;
            set => _SupportUserAccessMasterID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Controller;

        public string Controller
        {
            get => _Controller;
            set => _Controller = value;
        }

        private string _Action;

        public string Action
        {
            get => _Action.TrimIfNotNull();
            set => _Action = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _MenuGUID;

        public string MenuGUID
        {
            get => _MenuGUID.TrimIfNotNull();
            set => _MenuGUID = value.TrimIfNotNull();
        }
    }
}