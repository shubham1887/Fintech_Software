using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class MenuItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuItemID { get; set; }

        [Required]
        public string MenuItemName { get; set; } = string.Empty;

        public int UserTypeID { get; set; }

        public int Sequence {  get; set; }

        public string MappingCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;


    }
}
