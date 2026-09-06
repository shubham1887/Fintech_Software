using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SubscriptionMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionMasterID { get; set; }

        public DateTime StartDate { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string EndDate { get; set; }

        public DateTime UpdatedDOC { get; set; }

        public int UpdatedDOCInt { get; set; }
    }
}