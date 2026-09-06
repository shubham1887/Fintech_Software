using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderRequestResponseLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderRequestResponseLogID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster Usermaster { get; set; }

        public int UserID { get; set; }

        [Required]
        public int ProviderID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        [Required]
        public string Request { get; set; }

        [Required]
        public string Response { get; set; }

        [Required]
        public string IPAddress { get; set; }

        [Required]
        public string Through { get; set; }

        public int ServiceID { get; set; }
    }
}