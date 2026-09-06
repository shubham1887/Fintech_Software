using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OfflineProductMasterImages : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OfflineProductMasterImagesID { get; set; }

        public int OfflineProductMasterID { get; set; }

        [ForeignKey("OfflineProductMasterID")]
        public OfflineProductMaster OfflineProductMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ImageName { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Path { get; set; }
    }
}