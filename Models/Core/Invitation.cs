using FinancialPortalProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models
{
    public class Invitation
    {
        public int Id { get; set; }

        public int HouseHoldId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime Expires { get; set; }

        public bool Accepted { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(50)]
        public string EmailTo { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Body { get; set; }

        public Guid Code { get; set; } 

        public HouseHold HouseHold { get; set; }
    }
}
