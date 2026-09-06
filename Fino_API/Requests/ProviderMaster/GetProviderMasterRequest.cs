namespace FintechApp.Requests.ProviderMaster
{
    public class GetProviderMasterRequest
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
