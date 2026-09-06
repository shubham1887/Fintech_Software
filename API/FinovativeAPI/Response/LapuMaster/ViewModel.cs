using Project.Processing;

namespace Project.Response.LapuMaster
{
    public class ViewModel
    {
        private int _LapuPartyMasterID;

        public int LapuPartyMasterID
        {
            get => _LapuPartyMasterID;
            set => _LapuPartyMasterID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _LapuMasterID;

        public int LapuMasterID
        {
            get => _LapuMasterID;
            set => _LapuMasterID = value;
        }

        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }

        private string _ProivderLapuID;

        public string ProivderLapuID
        {
            get => _ProivderLapuID.TrimIfNotNull();
            set => _ProivderLapuID = value.TrimIfNotNull();
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _PartyName;

        public string PartyName
        {
            get => _PartyName.TrimIfNotNull();
            set => _PartyName = value.TrimIfNotNull();
        }
    }
}