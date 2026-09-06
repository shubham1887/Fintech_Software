using Project.Processing;
using Project.Requests.Sequence;
using System.Collections.Generic;

namespace Project.Response.Sequence
{
    public class OperatorSequenceResponse
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private List<ProviderList> _List;

        public List<ProviderList> ProviderList
        {
            get => _List;
            set => _List = value;
        }

        private string _Detail;
        public string Detail { get => _Detail; set => _Detail = value; }

        private int _RealProviderID;

        public int RealProviderID
        {
            get => _RealProviderID;
            set => _RealProviderID = value;
        }

        private int _ViewProviderID;

        public int ViewProviderID
        {
            get => _ViewProviderID;
            set => _ViewProviderID = value;
        }

        private int _OperatorSequenceUserWiseID;

        public int OperatorSequenceUserWiseID
        {
            get => _OperatorSequenceUserWiseID;
            set => _OperatorSequenceUserWiseID = value;
        }
    }
}