using Project.Processing;

namespace Project.Response.AEPS
{
    public class AEPSDistrictViewModel
    {
        private int _DistrictID;

        public int DistrictID
        {
            get => _DistrictID;
            set => _DistrictID = value;
        }

        private string _DistrictName;

        public string DistrictName
        {
            get => _DistrictName.TrimIfNotNull();
            set => _DistrictName = value.TrimIfNotNull();
        }
    }
}