using Project.Processing;

namespace Project.Response.AEPS
{
    public class AEPSStateViewModel
    {
        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }
    }
}