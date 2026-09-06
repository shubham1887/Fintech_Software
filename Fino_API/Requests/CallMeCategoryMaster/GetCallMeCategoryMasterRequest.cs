namespace FintechApp.Requests.CallMeCategoryMaster
{
    public class GetCallMeCategoryMasterRequest
    {
        public int CallMeCategoryMasterID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}