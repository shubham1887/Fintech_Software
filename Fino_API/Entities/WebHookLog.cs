using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class WebHookLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WebHookLogID { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string RequestURL { get; set; }

        [Required]
        public string Response { get; set; }
    }
}
