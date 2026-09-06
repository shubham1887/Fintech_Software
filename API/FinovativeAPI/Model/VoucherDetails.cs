using Project.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Project.Model
{
    public class VoucherDetails :BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherDetailsID { get; set; }
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }
        [Column(TypeName = "varchar(80)")]
        public string Mode { get; set; }

        public decimal OutstandingOpeningBal { get; set; }
        public decimal Amount{ get; set; }
        public decimal OutstandingClossingBal { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Remark { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Credit { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Debit { get; set; }
    }
}
