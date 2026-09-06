using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class EnquiryDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnquiryDetailsID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string PersonName { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string EmailID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string EnquiryType { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Remark { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string AdminStatus { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string AdminRemark { get; set; }
    }
}