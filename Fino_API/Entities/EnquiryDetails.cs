using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class EnquiryDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnquiryDetailsID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PersonName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string EmailID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string EnquiryType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Remark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string AdminStatus { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string AdminRemark { get; set; } = string.Empty;
    }
}
