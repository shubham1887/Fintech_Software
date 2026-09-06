using Newtonsoft.Json;
using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BlackListBankMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlackListBankMasterID { get; set; }


        private string _BankName;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        [Column(TypeName = "varchar(50)")]
        public string IFSC { get; set; }


        private string _AccountNumber;

        [JsonProperty("AccountNumber")]
        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }
    }
}
