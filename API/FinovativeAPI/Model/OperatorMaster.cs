using Project.Processing;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OperatorMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        private string _OperatorName;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumRechargeAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumRechargeAmount { get; set; }

        public int RepeatRechargeInMinute { get; set; }

        public bool? IsTimelyBlocked { get; set; }
        public DateTime BlockedTill { get; set; }

        public bool? IsRandom { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string OperatorImagePath { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Opcode { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string BusinessModel { get; set; }

        public int NumberLength { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string HSNCode { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Prefix { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccountStartWith { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string SequenceLogic { get; set; }

        [DefaultValue(false)]
        public bool? IsBBPS { get; set; }

        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string AccountName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string TollFreeNumber { get; set; }

        [DefaultValue(false)]
        public bool IsBillFetch { get; set; }

        [DefaultValue(false)]
        public bool IsPartialBill { get; set; }

        public decimal UISequence { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string BlockedAmount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string AllowedFor { get; set; }
        
        [DefaultValue(false)]
        public bool IsViewRequired { get; set; }
    }
}