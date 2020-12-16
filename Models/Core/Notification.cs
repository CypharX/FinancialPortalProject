using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.Core
{
    public class Notification
    {
        public int Id { get; set; }
        public int HouseHoldId { get; set; }
        public string FpUserId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Body { get; set; }

        public bool IsRead { get; set; }

        public HouseHold HouseHold { get; set; }
        public FpUser FpUser { get; set; }
    }
}
