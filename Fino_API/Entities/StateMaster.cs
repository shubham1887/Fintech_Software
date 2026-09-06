using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace FintechApp.Entities
{
    public class StateMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StateID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string StateName { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string StateCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}


//INSERT INTO StateMasters (StateName, StateCode, IsActive, DateTime, UpdatedateTime, IsDeleted)
//VALUES 
//('Andhra Pradesh', 'AP', 1, GETDATE(), GETDATE(), 0),
//('Arunachal Pradesh', 'AR', 1, GETDATE(), GETDATE(), 0),
//('Assam', 'AS', 1, GETDATE(), GETDATE(), 0),
//('Bihar', 'BR', 1, GETDATE(), GETDATE(), 0),
//('Chhattisgarh', 'CG', 1, GETDATE(), GETDATE(), 0),
//('Goa', 'GA', 1, GETDATE(), GETDATE(), 0),
//('Gujarat', 'GJ', 1, GETDATE(), GETDATE(), 0),
//('Haryana', 'HR', 1, GETDATE(), GETDATE(), 0),
//('Himachal Pradesh', 'HP', 1, GETDATE(), GETDATE(), 0),
//('Jharkhand', 'JH', 1, GETDATE(), GETDATE(), 0),
//('Karnataka', 'KA', 1, GETDATE(), GETDATE(), 0),
//('Kerala', 'KL', 1, GETDATE(), GETDATE(), 0),
//('Madhya Pradesh', 'MP', 1, GETDATE(), GETDATE(), 0),
//('Maharashtra', 'MH', 1, GETDATE(), GETDATE(), 0),
//('Manipur', 'MN', 1, GETDATE(), GETDATE(), 0),
//('Meghalaya', 'ML', 1, GETDATE(), GETDATE(), 0),
//('Mizoram', 'MZ', 1, GETDATE(), GETDATE(), 0),
//('Nagaland', 'NL', 1, GETDATE(), GETDATE(), 0),
//('Odisha', 'OR', 1, GETDATE(), GETDATE(), 0),
//('Punjab', 'PB', 1, GETDATE(), GETDATE(), 0),
//('Rajasthan', 'RJ', 1, GETDATE(), GETDATE(), 0),
//('Sikkim', 'SK', 1, GETDATE(), GETDATE(), 0),
//('Tamil Nadu', 'TN', 1, GETDATE(), GETDATE(), 0),
//('Telangana', 'TS', 1, GETDATE(), GETDATE(), 0),
//('Tripura', 'TR', 1, GETDATE(), GETDATE(), 0),
//('Uttar Pradesh', 'UP', 1, GETDATE(), GETDATE(), 0),
//('Uttarakhand', 'UK', 1, GETDATE(), GETDATE(), 0),
//('West Bengal', 'WB', 1, GETDATE(), GETDATE(), 0),
//('Andaman and Nicobar Islands', 'AN', 1, GETDATE(), GETDATE(), 0),
//('Chandigarh', 'CH', 1, GETDATE(), GETDATE(), 0),
//('Dadra and Nagar Haveli and Daman and Diu', 'DN', 1, GETDATE(), GETDATE(), 0),
//('Delhi', 'DL', 1, GETDATE(), GETDATE(), 0),
//('Jammu and Kashmir', 'JK', 1, GETDATE(), GETDATE(), 0),
//('Ladakh', 'LA', 1, GETDATE(), GETDATE(), 0),
//('Lakshadweep', 'LD', 1, GETDATE(), GETDATE(), 0),
//('Puducherry', 'PY', 1, GETDATE(), GETDATE(), 0);

