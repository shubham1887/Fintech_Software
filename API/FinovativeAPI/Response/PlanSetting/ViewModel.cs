using Project.Processing;

namespace Project.Response.PlanSetting
{
    public class ViewModel
    {
        private int _PlanSettingID;

        public int PlanSettingID
        {
            get => _PlanSettingID;
            set => _PlanSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _UserID;

        public string UserID
        {
            get => _UserID.TrimIfNotNull();
            set => _UserID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }
        
        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }


        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }
        private string _Circle;
        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
        }
        
    }
}