using FinancialPortalProject.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.ViewModels
{
    public class HouseHoldsDetailsVM
    {
       
        public HouseHold Household { get; set; }
        public List<Transaction> HhTransactions { get; set; } = new List<Transaction>();
        public List<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();
        public bool HasNotifcations { get; set; }
    }
}
