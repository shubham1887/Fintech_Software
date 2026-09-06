namespace Project.Response
{
    public class ProviderSequenceViewModel
    {
        private int _OperatorID;

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        private string _OperatorName;
        public string OperatorName { get => _OperatorName; set => _OperatorName = value; }

        private int _ServiceID;
        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }

        private string _ServiceName;
        public string ServiceName { get => _ServiceName; set => _ServiceName = value; }

        private int _ProviderID;
        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        private string _ProviderName;
        public string ProviderName { get => _ProviderName; set => _ProviderName = value; }

        private int _Sequence;
        public int Sequence { get => _Sequence; set => _Sequence = value; }

        private bool _IsRandom;
        public bool IsRandom { get => _IsRandom; set => _IsRandom = value; }
    }
}