using Project.Processing;

namespace Project.Requests.LeadGenerationMaster
{
    public class LoanTypes
    {
        private int _LoanTypeMasterID;
        public int LoanTypeMasterID
        {
            get => _LoanTypeMasterID;
            set => _LoanTypeMasterID = value;
        }
        private string _LoanType;
        public string LoanType
        {
            get => _LoanType.TrimIfNotNull();
            set => _LoanType = value.TrimIfNotNull();
        }
    }
}
