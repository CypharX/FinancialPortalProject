using FinancialPortalProject.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Services
{
    public interface IFP_NotifcationService
    {
        Task NotifyAsync(Transaction transaction, BankAccount account);
    }
}
