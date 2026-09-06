using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ThirdPartyServiceProvider : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ThirdPartyServiceProviderID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string Token { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string DomainName { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string AppID { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string AllowedServices { get; set; }

        [DefaultValue(false)]
        public bool IsActiveForWebsite { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }
    }
}