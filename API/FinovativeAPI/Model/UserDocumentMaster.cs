using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserDocumentMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserDocumentID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        public string DocmentType { get; set; }

        [Required]
        public string DocumentNo { get; set; }

        [Required]
        public string ImageURL { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }
    }
}