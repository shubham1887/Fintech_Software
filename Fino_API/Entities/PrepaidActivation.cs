namespace FintechApp.Entities
{
    public class PrepaidActivation : BaseEntity
    {
        public int PrepaidActivationID { get; set; }
        public int OperatorID { get; set; }
        public int UserID { get; set; }
        public string ActivationType { get; set; } = string.Empty;
        public string AlternateNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
