using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserCreationSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserCreationSettingID { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Cost { get; set; }

        public int MinimumCount { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
    }
}
