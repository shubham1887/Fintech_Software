using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserPaymentCollectMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserPaymentCollectMappingID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ProviderName { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Key { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; }
    }
}