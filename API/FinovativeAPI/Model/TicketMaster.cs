using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TicketMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketMasterID { get; set; }

        public int TicketCategoryID { get; set; }

        [ForeignKey("TicketCategoryID")]
        public TicketCategory TicketCategory { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string Subject { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Body { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string UploadFilePath { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Resolution { get; set; }

        public DateTime ResolutionDate { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster1 { get; set; }

        public int ResolutionByUserID { get; set; }

        public int UserTyepID { get; set; }

        [ForeignKey("UserTyepID")]
        public UserTypeMaster UserTypeMaster { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}