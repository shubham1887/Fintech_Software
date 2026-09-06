namespace FintechApp.Requests.CircleSwitching
{
    public class SelectCircleSwitchingRequest
    {
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
    }

    public class UpdateCircleSwitchingRequest
    {
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public List<CircleUpdateItem> Circles { get; set; } = new();
    }

    public class CircleUpdateItem
    {
        public int ProviderOperatorCircleMasterID { get; set; }
        public int CircleID { get; set; }
        public bool IsActive { get; set; }
        public string Opcode { get; set; } = string.Empty;
    }

    public class DeleteCircleSwitchingRequest
    {
        public int OperatorID { get; set; }
    }
}
