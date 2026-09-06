using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ViewProviderMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ViewProviderMappingID { get; set; }

        public int ProviderID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }
    }
}
