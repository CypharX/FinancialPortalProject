using FinancialPortalProject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.Core
{
    public class Transaction
    {
        public int Id { get; set; }
        public int BankAccountId { get; set; }
        public int? CategoryItemId { get; set; }
        public string FpUserId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [Required]
        [Display (Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }


        [StringLength(50)]
        public string Memo { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; }

        public BankAccount BankAccount { get; set; }
        public FpUser FpUser { get; set; }
        public CategoryItem CategoryItem { get; set; }
    }
}
