using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class LOB_Details
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LOB_MasterID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string TypeOfShop { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string LineOfBusiness { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string LOBCode { get; set; }
    }
}
