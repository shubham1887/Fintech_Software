using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class EmailSettingMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailSettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Email
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Password
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HostName
        {
            get; set;
        }

        [Required]
        public int Port
        {
            get; set;
        }

        public bool IsSSL
        {
            get; set;
        }

        public bool IsActive
        {
            get; set;
        }
    }
}