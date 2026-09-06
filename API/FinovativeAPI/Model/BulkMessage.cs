using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BulkMessage : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BulkMessageID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Message { get; set; }

        public bool IsSMS { get; set; }

        public bool IsEmail { get; set; }
        public bool IsWhatsUp { get; set; }

        public bool IsDelete { get; set; }
    }
}