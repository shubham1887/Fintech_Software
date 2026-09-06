using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SupportUserAccessMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupportUserAccessMasterID { get; set; }

        public int MenuMasterID { get; set; }
        public int UserID { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MenuGUID { get; set; }
    }
}