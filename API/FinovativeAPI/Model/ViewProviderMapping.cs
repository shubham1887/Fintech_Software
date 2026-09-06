using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ViewProviderMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ViewProviderMappingID { get; set; }

        public int ProviderID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }
    }
}