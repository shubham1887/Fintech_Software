using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class DMTBank : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DMTBankID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        [Column(TypeName = "varchar(50)")]
        public string DefaultIFSC { get; set; }

        public bool IsActive { get; set; }

        [DefaultValue(true)]
        public bool IsIMPS { get; set; }
    }
}