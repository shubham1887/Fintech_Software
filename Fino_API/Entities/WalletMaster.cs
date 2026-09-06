using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class WalletMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WalletID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ShortName { get; set; } = String.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }  = String.Empty;
        [DefaultValue(0)]
        public int UISequence { get; set; }

        public bool IsActive { get; set; }

        

     
    }
}
