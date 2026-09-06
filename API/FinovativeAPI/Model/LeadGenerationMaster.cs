using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LeadGenerationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeadGenerationMasterID { get; set; }
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FormID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MerchantName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string RetailerName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ConsumerName { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string EmailID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AadharNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string PanNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ConsumerType { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string RequiredAmount { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LoanType { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ApprovedAmount { get; set; }
        
        [Column(TypeName = "varchar(50)")]
        public string Gender { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string APIUserRequestID { get; set; }

        public DateTime DOB { get; set; }

    }
}
