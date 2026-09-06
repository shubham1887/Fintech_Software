using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LapuPartyMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LapuPartyMasterID { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string PartyName { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(100)")]
        public string City { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(100)")]
        public string State { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "varchar(70)")]
        public string EmailID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string MobileNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
