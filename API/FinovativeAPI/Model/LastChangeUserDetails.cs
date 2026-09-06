using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LastChangeUserDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LastChangeUserDetailsID { get; set; }
        public int OnboardingUserID { get; set; }
        
        [Column(TypeName = "varchar(500)")]
        public string Change { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string OldChanges { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string NewChanges { get; set; }
      


    }
}
