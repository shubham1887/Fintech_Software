using Project.Processing;

namespace Project.Response.OperatorStateMapping
{
    public class ViewModel
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _HLR;

        public string HLR
        {
            get => _HLR.TrimIfNotNull();
            set => _HLR = value.TrimIfNotNull();
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private bool isActive;
        public bool IsActive { get => isActive; set => isActive = value; }

        private bool _IsDelete;
        public bool IsDelete { get => _IsDelete; set => _IsDelete = value; }
        public int OperatorStateMappingID { get; internal set; }
    }
}