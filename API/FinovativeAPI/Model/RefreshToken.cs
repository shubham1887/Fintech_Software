using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class RefreshTokenMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefreshTokenID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string RefreshToken { get; set; }

        public DateTime Expires { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }
    }
}