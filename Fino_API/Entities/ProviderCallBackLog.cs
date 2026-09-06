using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderCallBackLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderCallBackID { get; set; }

        public string ProviderReply { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public string Type { get; set; } = string.Empty;
        public int TransactionMasterID { get; set; }
    }
}
