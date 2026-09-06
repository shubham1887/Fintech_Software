using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LocationPinCodeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SR_NO { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PINCODE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_DISTRICT { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HYP_STATE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_TALUKA_CITY { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string REGIONNAME { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_REGION { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_ZONE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_AREA { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_NSM { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_RH { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_ZSM { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_SH { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_CM { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_ASM { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HPY_FLS { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ACTIVATED { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ACTIVATION_DATE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string LOCATION { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string WITHIN_CITY_LIMITS { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DISTANCE_FROM_CBD { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string INDIAPOST_OFFICE_NAME { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DIVISIONNAME { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string LONGITUDE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string LATITUDE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_CODE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_NAME { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_MOBILE { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string EMPLOYEE_EMAIL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_5 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_6 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_7 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_8 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_9 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_10 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_11 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_12 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_13 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_14 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RESERVEFIELD_15 { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}
