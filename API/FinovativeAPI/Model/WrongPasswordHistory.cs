using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class WrongPasswordHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WrongPasswordHistoryID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Mobile number must be of 10 digit.")]
        [Column(TypeName = "varchar(10)")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string IPAddress { get; set; }
    }
}