using EcollectionDetailsEntity = FintechApp.Entities.EcollectionDetails;

namespace FintechApp.Responses.Fund
{
    public class EcollectionWrapper
    {
        public int Count { get; set; }
        public List<EcollectionDetailsEntity> list { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
    }
}
