using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LapuTransactionDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LapuTransactionDetailID { get; set; }

        [DefaultValue(0)]
        public int UserID { get; set; }

        [DefaultValue(0)]
        public int OperatorID { get; set; }

        [DefaultValue(0)]
        public int ProivderID { get; set; }

        [DefaultValue(0)]
        public int LapuMasterID { get; set; }

        [DefaultValue(0)]
        public int LapuPartyMasterID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string LapuNo { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ConsumerNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Commission { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DOCType { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; }

        public DateTime TxnDate { get; set; }
    }
}