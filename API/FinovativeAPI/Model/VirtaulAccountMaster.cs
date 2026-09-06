using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class VirtualAccountMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VirtualAccountMasterID { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Prefix must be of 10 digit.")]
        [Column(TypeName = "varchar(10)")]
        public string Prefix { get; set; }

        private string _BankName;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _IFSC;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
    }
}