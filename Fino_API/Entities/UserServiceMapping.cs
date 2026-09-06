using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserServiceMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserServiceMappingID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        /// <summary>
        /// The user holds this service at all. Combined with <see cref="IsActive"/> this stores the
        /// three states of the Add / Edit User services grid:
        /// On = IsGranted + IsActive, Lock = IsGranted only, Off = neither.
        /// A row is written for every service once an admin saves the grid, so "the whole grid is
        /// Off" is stored as real rows rather than as no rows (which means "never configured").
        /// </summary>
        public bool IsGranted { get; set; } = true;

        public bool IsActive { get; set; }
    }
}
