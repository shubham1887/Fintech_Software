using Project.Processing;

namespace Project.Response.DenominationMaster
{
    public class ViewModel
    {
        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
        }

        private string _DenominationRange;

        public string DenominationRange
        {
            get => _DenominationRange.TrimIfNotNull();
            set => _DenominationRange = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsStop;
        public bool IsStop { get => _IsStop; set => _IsStop = value; }

        public string IsStopStr
        {
            get => (IsStop) ? "Yes" : "No";
        }
    }
}