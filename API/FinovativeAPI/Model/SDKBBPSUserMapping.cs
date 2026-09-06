using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SDKBBPSUserMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SDKBBPSUserMappingID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ConfirmationCallbackURL { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}