using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class DeviceWiseFailLoginHistory : BaseEntity
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceWiseFailLoginHistoryID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string UserName { get; set; }
        
        [Required]
        [Column(TypeName = "varchar(500)")]
        public string DeviceID { get; set; }

    }
}
