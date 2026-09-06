using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class OnboardingUserDocument : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OnboardingUserDocumentID { get; set; }
      
        [Column(TypeName = "varchar(500)")]
        public string Ref_ID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public int Type { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string OldImage { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string NewImage { get; set; }

        public int OnboardingUserID { get; set; }

        [ForeignKey("OnboardingUserID")]
        public OnboardingUser OnboardingUser { get; set; }
    }
}
