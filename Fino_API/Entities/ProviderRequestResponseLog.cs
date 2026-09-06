using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderRequestResponseLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderRequestResponseLogID { get; set; }

        // Null for account-level enquiries (balance check) that aren't tied to a user.
        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int ProviderID { get; set; }
        public int TransactionID { get; set; }
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public int ServiceID { get; set; }
    }
}
