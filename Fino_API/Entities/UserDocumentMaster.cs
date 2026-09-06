using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserDocumentMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserDocumentID { get; set; }
        public int UserID { get; set; }
        public string DocmentType { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        [Column(TypeName = "varchar(100)")] public string? Remark { get; set; }
        [Column(TypeName = "varchar(20)")] public string? Status { get; set; }
        public virtual UserMaster? UserMaster { get; set; }
    }
}
