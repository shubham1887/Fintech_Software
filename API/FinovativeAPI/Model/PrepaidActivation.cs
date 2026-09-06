using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PrepaidActivation : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PrepaidActivationID { get; set; }

        public int OperatorID { get; set; }
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ActivationType { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string AlternateNumber { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Pincode { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; }
    }
}