namespace Project.Response.UnitMaster
{
    public class ViewModel
    {
        private int _UnitMasterID;

        public int UnitMasterID
        {
            get => _UnitMasterID;
            set => _UnitMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name;
            set => _Name = value;
        }
    }
}