using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinancialPortalProject.Data;
using FinancialPortalProject.Models.Core;
using Microsoft.AspNetCore.Identity;
using FinancialPortalProject.Models;
using FinancialPortalProject.Enums;
using FinancialPortalProject.Services;

namespace FinancialPortalProject.Controllers
{
    public class BankAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FpUser> _userManager;
        private readonly IFP_NotifcationService _notifcationService;

        public BankAccountsController(ApplicationDbContext context, UserManager<FpUser> userManager, IFP_NotifcationService notifcationService)
        {
            _context = context;
            _userManager = userManager;
            _notifcationService = notifcationService;
        }

        // GET: BankAccounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BankAccounts.Include(b => b.HouseHold).Include(b => b.Owner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BankAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts
                .Include(b => b.HouseHold)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }

            return View(bankAccount);
        }

        // GET: BankAccounts/Create
        public IActionResult Create()
        {
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name");
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: BankAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HouseHoldId,Name,AccountType,StartingBalance,LowBalance")] BankAccount bankAccount)
        {
            if (ModelState.IsValid)
            {
                bankAccount.OwnerId = _userManager.GetUserId(User);
                bankAccount.CurrentBalance = bankAccount.StartingBalance;                
                _context.Add(bankAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "HouseHolds", new { id = bankAccount.HouseHoldId});
            }
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", bankAccount.HouseHoldId);
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "FullName", bankAccount.OwnerId);
            return View(bankAccount);
        }

        // GET: BankAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts.FindAsync(id);
            if (bankAccount == null)
            {
                return NotFound();
            }
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", bankAccount.HouseHoldId);
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "Id", bankAccount.OwnerId);
            return View(bankAccount);
        }

        // POST: BankAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HouseHoldId,FpUserId,Name,AccountType,StartingBalance,CurrentBalance")] BankAccount bankAccount)
        {
            if (id != bankAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bankAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankAccountExists(bankAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", bankAccount.HouseHoldId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", bankAccount.OwnerId);
            return View(bankAccount);
        }

        // GET: BankAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts
                .Include(b => b.HouseHold)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }

            return View(bankAccount);
        }

        // POST: BankAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);
            _context.BankAccounts.Remove(bankAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BankAccountExists(int id)
        {
            return _context.BankAccounts.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var bankAccount = await _context.BankAccounts.FindAsync(id);
            bankAccount.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "HouseHolds", new { id = bankAccount.HouseHoldId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferFunds(int sendingAccountId, int receivingAccountId, decimal amount)
        {
            var sendingAccount = await _context.BankAccounts.FindAsync(sendingAccountId);
            var receivingAccount = await _context.BankAccounts.FindAsync(receivingAccountId);
            sendingAccount.CurrentBalance -= amount;
            receivingAccount.CurrentBalance += amount;
            var user = await _userManager.GetUserAsync(User);
            await _context.SaveChangesAsync();

            var outTransaction = new Transaction
            {
                BankAccountId = sendingAccount.Id,
                FpUserId = user.Id,
                Created = DateTime.Now,
                TransactionType = TransactionType.Withdrawal,
                Memo = $"Money transfered to {receivingAccount.Name}",
                Amount = amount
            };

            var inTransaction = new Transaction
            {
                BankAccountId = receivingAccount.Id,
                FpUserId = user.Id,
                Created = DateTime.Now,
                TransactionType = TransactionType.Deposit,
                Memo = $"Money received from {sendingAccount.Name}",
                Amount = amount
            };
            _context.Add(inTransaction);
            _context.Add(outTransaction);
            await _context.SaveChangesAsync();

            await _notifcationService.NotifyAsync(outTransaction, sendingAccount);
            return RedirectToAction("Details", "HouseHolds", new { id = user.HouseHoldId });
        }
    }
}
