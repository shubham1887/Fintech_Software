using Project.Processing;

namespace Project.Requests
{
    public class KeyDataModelForLogout
    {
        private string _UserID;

        public string UserID
        {
            get => _UserID.TrimIfNotNull();
            set => _UserID = value.TrimIfNotNull();
        }
    }
}