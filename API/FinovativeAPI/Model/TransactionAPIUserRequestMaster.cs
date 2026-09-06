using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    [Index(nameof(UserID), nameof(ApiUserRequestID), Name = "IX_UserID_ApiRequestID", IsUnique = true)]
    public class TransactionAPIUserRequestMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionAPIUserRequestMasterID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ApiUserRequestID { get; set; }
    }
}
