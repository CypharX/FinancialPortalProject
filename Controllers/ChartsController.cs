using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialPortalProject.Data;
using FinancialPortalProject.Enums;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortalProject.Controllers
{
    public class ChartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FpUser> _userManager;
        private readonly List<string> BgColors;

        public ChartsController(ApplicationDbContext context, UserManager<FpUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            BgColors = new List<string>
            {               
                "#25d5e4",
                "#e95f2b",
                "#304aca",
                "#009688",
                "#f8538d",
                "#ffbb44",
                "#445ede",
                "#5c1ac3"
            };
        }

        public async Task<JsonResult> MoneyPerMember()
        {
            var result = new DonutChartModel();
            var user = await _userManager.GetUserAsync(User);
            var members = _context.Users.Where(u => u.HouseHoldId == user.HouseHoldId).ToList();
            var count = 0;
            foreach (var member in members)
            {
                result.Labels.Add(member.FullName);
                var accounts = _context.BankAccounts.Where(ba => ba.OwnerId == member.Id && ba.IsDeleted == false).ToList();
                decimal accountTotal = 0;
                foreach (var account in accounts)
                {
                    accountTotal += account.CurrentBalance;
                }
                result.Data.Add(accountTotal);
                if(count >= BgColors.Count())
                {
                    count = 0;
                }
                result.BackgroundColor.Add(BgColors[count]);
                count++;
            }
            return Json(result);
        }

        public async Task<JsonResult> TransactionsPerMember()
        {
            var result = new DonutChartModel();
            var user = await _userManager.GetUserAsync(User);
            var members = _context.Users.Where(u => u.HouseHoldId == user.HouseHoldId).ToList();
            var count = 1;
            foreach (var member in members)
            {
                result.Labels.Add(member.FullName);
                var transactions = _context.Transactions
                    .Where(t => t.FpUserId == member.Id && t.IsDeleted == false && t.TransactionType == TransactionType.Withdrawal).ToList();
                decimal transactionTotal = 0;
                foreach (var transaction in transactions)
                {
                    transactionTotal += transaction.Amount;
                }
                result.Data.Add(transactionTotal);
                if (count >= BgColors.Count())
                {
                    count = 0;
                }
                result.BackgroundColor.Add(BgColors[count]);
                count++;
            }
            return Json(result);
        }

        public async Task<JsonResult> TransactionsPerCategory()
        {
            var result = new DonutChartModel();
            var user = await _userManager.GetUserAsync(User);
            var categories = _context.Categories
                .Include(c => c.CategoryItems)
                .Where(c => c.HouseHoldId == user.HouseHoldId && c.IsDeleted == false && c.Name != "Deposits")
                .ToList();
            var count = 4;
            foreach (var category in categories)
            {
                result.Labels.Add(category.Name);
                var transactions = _context.Transactions
                    .Where(t => t.CategoryItem.CategoryId == category.Id && t.IsDeleted == false && t.TransactionType == TransactionType.Withdrawal)
                    .ToList();
                decimal transactionTotal = 0;
                foreach (var transaction in transactions)
                {
                    transactionTotal += transaction.Amount;
                }
                result.Data.Add(transactionTotal);
                if (count >= BgColors.Count())
                {
                    count = 0;
                }
                result.BackgroundColor.Add(BgColors[count]);
                count++;
            }
            return Json(result);
        }
    }
}
