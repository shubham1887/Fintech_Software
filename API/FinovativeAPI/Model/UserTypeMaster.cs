using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserTypeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserTypeID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UserType { get; set; }

        public bool IsOutside { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string AliasName { get; set; }

        [DefaultValue(1)]
        [Column(TypeName = "decimal(18,5)")]
        public decimal FundTransferMinimumAmount { get; set; }
    }
}