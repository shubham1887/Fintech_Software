using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LocationLookUp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationLookUpID { get; set; }

        public int TableID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string SimCardDetails { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string Type { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Latitude { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Longitude { get; set; } = string.Empty;
    }
}
