using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class MenuMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuMasterID { get; set; }

        public decimal MenuSequence { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Url { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Icon { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
        public bool IsSuperDistributer { get; set; }
        public bool IsDistributer { get; set; }
        public bool IsRetailer { get; set; }
        public bool IsAPIUser { get; set; }
        public bool IsSupport { get; set; }
        public int Level { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string GUID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ParentMenuGUID { get; set; } = string.Empty;
    }
}
