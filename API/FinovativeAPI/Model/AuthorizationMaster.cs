using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class AuthorizationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorizationMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Controller { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Action { get; set; }

        public int UserTypeID { get; set; }

        [ForeignKey("UserTypeID")]
        public UserTypeMaster UserTypeMaster { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}