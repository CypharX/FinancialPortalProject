using FinancialPortalProject.Data;
using FinancialPortalProject.Enums;
using FinancialPortalProject.Models;
using FinancialPortalProject.Models.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Services
{
    public class FP_NotifcationService : IFP_NotifcationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly UserManager<FpUser> _userManager;

        public FP_NotifcationService(ApplicationDbContext context, IEmailSender emailSender, UserManager<FpUser> userManager)
        {
            _context = context;
            _emailService = emailSender;
            _userManager = userManager;
        }

        public async Task NotifyAsync(Transaction transaction, BankAccount account)
        {
            var user = await _context.Users.FindAsync(transaction.FpUserId);
            var hhMembers = await _context.Users.Where(u => u.HouseHoldId == account.HouseHoldId).ToListAsync();
            var notification = new Notification();
            //var formattedTransaction = String.Format("{0:C}", transaction.Amount);
            //var formattedBalance = String.Format("{0:C}", account.CurrentBalance);
            if (account.CurrentBalance < 0 && account.CurrentBalance + transaction.Amount > 0)
            {
               
                notification.Created = DateTime.Now;
                notification.HouseHoldId = account.HouseHoldId;
                notification.FpUserId = user.Id;
                notification.Subject = $"Your account {account.Name} has been overdrafted";
                notification.Body = $"{user.FullName} has overdrafted the account {account.Name} on {transaction.Created:MMM dd yyyy} with a purchase of ${transaction.Amount}";
               
            }
            else if(account.CurrentBalance < account.LowBalance)
            {
                notification.Created = DateTime.Now;
                notification.HouseHoldId = account.HouseHoldId;
                notification.FpUserId = user.Id;
                notification.Subject = $"Your account {account.Name} has been fallen below your low balance alert";
                notification.Body = $"{user.FullName} has made a transaction on {transaction.Created:MMM dd yyyy} in the amount of ${transaction.Amount} lowering your account balance to ${account.CurrentBalance} which is below your low balance alert";                             
            }
            if(!string.IsNullOrWhiteSpace(notification.Subject))
            {
                _context.Add(notification);         
                await _context.SaveChangesAsync();
                foreach(var member in hhMembers)
                {
                    if(await _userManager.IsInRoleAsync(member, nameof(Roles.Head)))
                    {
                        await _emailService.SendEmailAsync(member.Email, notification.Subject, notification.Body);
                    }
                    else if(notification.FpUserId == member.Id)
                    {
                        await _emailService.SendEmailAsync(member.Email, notification.Subject, notification.Body);
                    }
                }
            }
           
        }
    }
}
