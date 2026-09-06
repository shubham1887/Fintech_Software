using FintechApp.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CallMeCategoryMaster : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CallMeCategoryMasterID { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string Name { get; set; }
}