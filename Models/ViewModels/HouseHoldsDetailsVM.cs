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
    }
}
