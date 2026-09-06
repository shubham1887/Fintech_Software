using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FintechApp.Helpers;

namespace FintechApp.Entities
{
    public class TicketCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketCategoryID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }
    }
}
