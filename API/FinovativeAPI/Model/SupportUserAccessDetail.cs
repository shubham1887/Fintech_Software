using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SupportUserAccessDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupportUserAccessDetailID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Controller { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Action { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MenuGUID { get; set; }
    }
}