using Project.Processing;

namespace Project.Response.User
{
    public class StateViewModel
    {
        private int _StateID;
        private string _Name;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}