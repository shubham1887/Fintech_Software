using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LapuPartyMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LapuPartyMasterID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string PartyName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string City { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string State { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string EmailID { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Mobile number must be of 10 digit.")]
        [Column(TypeName = "varchar(10)")]
        public string MobileNumber { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }
    }
}