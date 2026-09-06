using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserTypeMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserTypeID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UserType { get; set; }
       
        [Column(TypeName = "varchar(10)")]
        public string AliasName { get; set; }
        public bool IsActive { get; set; }


    }
}
