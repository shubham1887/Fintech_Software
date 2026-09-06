using System.Collections.Generic;

namespace Project.Response.ProviderOperatorDenominationMapping
{
    public class DenominationSwitchingModel
    {
        private int _OperatorID;

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        private string _OperatorName;
        public string OperatorName { get => _OperatorName; set => _OperatorName = value; }

        public List<APIModel> APIs = new List<APIModel>();
    }

    public class APIModel
    {
        private int _ProviderID;

        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        private string _ProviderName;
        public string ProviderName { get => _ProviderName; set => _ProviderName = value; }
    }
}