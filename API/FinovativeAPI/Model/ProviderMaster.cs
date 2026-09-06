using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderID { get; set; }

        private string _ProviderName;

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string ProviderName
        {
            get => _ProviderName.ToTitleCase();
            set => _ProviderName = value.ToTitleCase();
        }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProviderUrl { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string ProviderUniqueID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string IPAddress { get; set; }

        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsHLR { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RechargeBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DMTBalance { get; set; }

        public int ResponseGroupID { get; set; }

        public bool IsRealAPI { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string LoginID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string SupportNumber { get; set; }

        public int LowBalanceAmount { get; set; }
    }
}