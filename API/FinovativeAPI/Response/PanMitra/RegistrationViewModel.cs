using Project.Processing;

namespace Project.Response.PanMitra
{
    public class RegistrationViewModel
    {
        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
        
        private string _EmailID;
        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private string _VleID;
        public string VleID
        {
            get => _VleID.TrimIfNotNull();
            set => _VleID = value.TrimIfNotNull();
        }
        private string _Status;
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }


    }
}
