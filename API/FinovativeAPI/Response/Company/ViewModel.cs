namespace Project.Response.Company
{
    public class ViewModel
    {
        private int _CompanyMasterID;

        public int CompanyMasterID
        {
            get => _CompanyMasterID;
            set => _CompanyMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        private string _Value;

        public string Value
        {
            get => _Value;
            set => _Value = value;
        }
    }
}