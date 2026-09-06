using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LocationLookUp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationLookUpID { get; set; }

        public int TableID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string SimCardDetails { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Location { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Latitude { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Longitude { get; set; }
    }
}