using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class FingerPrintDeviceType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FingerPrintDeviceTypeID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DeviceName { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Value { get; set; }
    }
}