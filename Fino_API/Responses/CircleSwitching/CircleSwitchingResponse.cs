namespace FintechApp.Responses.CircleSwitching
{
    public class CircleSwitchingOperatorResponse
    {
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public string Operator { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public List<CircleDetailResponse> Circles { get; set; } = new();
    }

    public class CircleDetailResponse
    {
        public int ProviderOperatorCircleMasterID { get; set; }
        public int CircleID { get; set; }
        public string CircleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Opcode { get; set; } = string.Empty;
    }
}
