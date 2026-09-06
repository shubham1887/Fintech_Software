using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class KYCApprovedDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KYCApprovedDetailsID { get; set; }

        public int UserID { get; set; }

        public int ApporvedUserID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Remark { get; set; }
    }
}
