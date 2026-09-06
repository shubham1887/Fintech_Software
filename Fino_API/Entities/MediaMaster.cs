using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class MediaMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaMasterID { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MediaType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string MediaPath { get; set; } = string.Empty;
    }
}
