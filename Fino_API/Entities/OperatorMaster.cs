using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } 

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string OperatorName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumRechargeAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumRechargeAmount { get; set; }

        public int RepeatRechargeInMinute { get; set; }

        public DateTime BlockedTill { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string OperatorImagePath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Opcode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string BusinessModel { get; set; } = string.Empty;

        public int NumberLength { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string HSNCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Prefix { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string AccountStartWith { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string SequenceLogic { get; set; } = string.Empty;

        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string AccountName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string TollFreeNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UISequence { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string BlockedAmount { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string AllowedFor { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }

        [DefaultValue(false)]
        public bool? IsTimelyBlocked { get; set; }

        [DefaultValue(false)]
        public bool? IsRandom { get; set; }

        [DefaultValue(false)]
        public bool? IsBBPS { get; set; }

        [DefaultValue(false)]
        public bool IsBillFetch { get; set; }

        [DefaultValue(false)]
        public bool IsPartialBill { get; set; }

        [DefaultValue(false)]
        public bool IsViewRequired { get; set; }

        // When true, the provider-operator denomination mappings are enforced while routing a
        // recharge for this operator; when false the operator ignores denominations entirely.
        [DefaultValue(false)]
        public bool IsDenominationApplicable { get; set; }
    }
}