using Project.Processing;

namespace Project.Response.UserActionHistory
{
    public class ViewModel
    {
        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }

        private string _FirmName;
        public string FirmName { get => _FirmName.TrimIfNotNull(); set => _FirmName = value.TrimIfNotNull(); }

        private string _Action;
        public string Action { get => _Action.TrimIfNotNull(); set => _Action = value.TrimIfNotNull(); }
        private string _Description;
        public string Description { get => _Description.TrimIfNotNull(); set => _Description = value.TrimIfNotNull(); }

        private string _DOC;
        public string DOC { get => _DOC.TrimIfNotNull(); set => _DOC = value.TrimIfNotNull(); }

        private string _Status;
        public string Status { get => _Status.TrimIfNotNull(); set => _Status = value.TrimIfNotNull(); }
    }
}