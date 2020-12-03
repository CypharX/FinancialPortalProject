using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.Core
{
    public class HouseHold
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(150)]
        public string Greeting { get; set; }

        public DateTime? Established { get; set; }

        public virtual ICollection<FpUser> Members { get; set; } = new HashSet<FpUser>();
        public virtual ICollection<Invitation> Invitations { get; set; } = new HashSet<Invitation>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new HashSet<BankAccount>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
