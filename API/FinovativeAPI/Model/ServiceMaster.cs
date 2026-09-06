using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ServiceMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name
        {
            get => _Name.TrimIfNotNull(); set => _Name = value.TrimIfNotNull();
        }

        public bool IsDelete { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinBalance { get; set; }

        [DefaultValue(true)]
        public bool IsDisplay { get; set; }

        [DefaultValue(true)]
        public bool IsDefault { get; set; }

        private string _ServiceCategory;

        [Column(TypeName = "varchar(100)")]
        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull(); set => _ServiceCategory = value.TrimIfNotNull();
        }

        private string _HSNCode;

        [Column(TypeName = "varchar(200)")]
        public string HSNCode
        {
            get => _HSNCode.TrimIfNotNull(); set => _HSNCode = value.TrimIfNotNull();
        }

        private string _HSNCodeForSelfInvoice;

        [Column(TypeName = "varchar(100)")]
        public string HSNCodeForSelfInvoice
        {
            get => _HSNCodeForSelfInvoice.TrimIfNotNull(); set => _HSNCodeForSelfInvoice = value.TrimIfNotNull();
        }

        [DefaultValue(true)]
        public bool IsOutside { get; set; }

        [DefaultValue(false)]
        public bool? IsBBPS { get; set; }

        [DefaultValue(true)]
        public bool IsSubscriptionAllow { get; set; }


    }
}