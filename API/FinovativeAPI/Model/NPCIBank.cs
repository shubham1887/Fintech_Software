using MplanCore.Controller;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class NPCIBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NPCIBankID { get; set; }
     
        [Column(TypeName = "varchar(50)")]
        public string BankCode { get; set; }
       
        [Required]
        [Column(TypeName = "varchar(200)")]
        public string BankName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string IFSC { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string IIN { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string ParentBank { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string BankType { get; set; }

        public bool IsActive { get; set; }
    }
}
