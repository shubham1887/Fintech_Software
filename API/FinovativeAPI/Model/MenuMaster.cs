using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class MenuMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuMasterID { get; set; }

        public decimal MenuSequence { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Url { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Icon { get; set; }

        public string Description { get; set; }

        [DefaultValue(false)]
        public bool IsAdmin { get; set; }

        [DefaultValue(false)]
        public bool IsSuperDistributer { get; set; }

        [DefaultValue(false)]
        public bool IsDistributer { get; set; }

        [DefaultValue(false)]
        public bool IsRetailer { get; set; }

        [DefaultValue(false)]
        public bool IsAPIUser { get; set; }

        [DefaultValue(false)]
        public bool IsSupport { get; set; }

        public int Level { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string GUID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ParentMenuGUID { get; set; }
    }
}