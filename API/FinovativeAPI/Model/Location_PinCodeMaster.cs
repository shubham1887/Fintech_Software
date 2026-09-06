using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class Location_PinCodeMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SR_NO { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string PINCODE { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_DISTRICT { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HYP_STATE { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_TALUKA_CITY { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string REGIONNAME { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_REGION { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_ZONE { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_AREA { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_NSM { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_RH { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_ZSM { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_SH { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_CM { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_ASM { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HPY_FLS { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ACTIVATED { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ACTIVATION_DATE { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LOCATION { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string WITHIN_CITY_LIMITS { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string DISTANCE_FROM_CBD { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string INDIAPOST_OFFICE_NAME { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string DIVISIONNAME { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LONGITUDE { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LATITUDE { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_CODE { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_NAME { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_MOBILE { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_EMAIL { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_5 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_6 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_7 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_8 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_9 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_10 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_11 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_12 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_13 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_14 { get; set; }


        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_15 { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
