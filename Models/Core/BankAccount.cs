using FinancialPortalProject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.Core
{
    public class BankAccount
    {
        public int Id { get; set; }
        public int HouseHoldId { get; set; }
        public string OwnerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        [EnumDataType(typeof(AccountType))]
        public AccountType AccountType { get; set; }


        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(9, 2)")]
        public decimal StartingBalance { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(9, 2)")]
        public decimal CurrentBalance { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(9, 2)")]
        public decimal LowBalance { get; set; }

        public bool IsDeleted { get; set; }

        public HouseHold HouseHold { get; set; }
        public FpUser Owner { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
