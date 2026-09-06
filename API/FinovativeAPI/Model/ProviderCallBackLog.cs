using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderCallBackLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderCallBackID { get; set; }

        [Required]
        public string ProviderReply { get; set; }

        [Required]
        public int ProviderID { get; set; }

        [Required]
        public string Type { get; set; }

        public int TransactionMasterID { get; set; }
    }
}