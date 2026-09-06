using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LoginHistrory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginHistoryID { get; set; }

        public int UserID
        {
            get; set;
        }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string IPAddress
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Token
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Through
        {
            get; set;
        }

        public DateTime Date
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string LoginMessage
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Location
        {
            get; set;
        }

        [Column(TypeName = "varchar(max)")]
        public string SimCardDetails
        {
            get; set;
        }
    }
}