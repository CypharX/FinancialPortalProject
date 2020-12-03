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
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(50)]
        public string Memo { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(7, 2)")]
        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; }

        public BankAccount BankAccount { get; set; }
        public FpUser User { get; set; }
        public CategoryItem CategoryItem { get; set; }
    }
}
