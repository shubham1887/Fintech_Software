using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.CircleSwitching
{
    public class ViewModel
    {
        public int OperatorID { get; set; }
        public string Operator { get; set; }
        public int ProviderID { get; set; }
        public int StateID { get; set; }
        public int ProviderOperatorCircleMasterID { get; set; }
        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        public List<OperatorCircleMaster> Circles = new List<OperatorCircleMaster>();
    }
}