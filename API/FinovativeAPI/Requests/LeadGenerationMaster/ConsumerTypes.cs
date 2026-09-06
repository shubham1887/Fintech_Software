using Project.Processing;

namespace Project.Requests.LeadGenerationMaster
{
    public class ConsumerTypes
    {
        private int _ConsumerTypeMasterID;
        public int ConsumerTypeMasterID
        {
            get => _ConsumerTypeMasterID;
            set => _ConsumerTypeMasterID = value;
        }
        private string _ConsumerType;
        public string ConsumerType
        {
            get => _ConsumerType.TrimIfNotNull();
            set => _ConsumerType = value.TrimIfNotNull();
        }
    }
}
