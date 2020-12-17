using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinancialPortalProject.Data;
using FinancialPortalProject.Models.Core;
using FinancialPortalProject.Enums;
using Microsoft.AspNetCore.Identity;
using FinancialPortalProject.Models;
using FinancialPortalProject.Services;
using Microsoft.AspNetCore.Authorization;

namespace FinancialPortalProject.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FpUser> _userManager;
        private readonly IFP_NotifcationService _notifcationService;

        public TransactionsController(ApplicationDbContext context, UserManager<FpUser> userManager, IFP_NotifcationService notifcationService)
        {
            _context = context;
            _userManager = userManager;
            _notifcationService = notifcationService;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.BankAccount).Include(t => t.CategoryItem).Include(t => t.FpUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.BankAccount)
                .Include(t => t.CategoryItem)
                .Include(t => t.FpUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["BankAccountId"] = new SelectList(_context.BankAccounts, "Id", "Name");
            ViewData["CategoryItemId"] = new SelectList(_context.CategoryItems, "Id", "Description");
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BankAccountId,CategoryItemId,Memo,Amount")] Transaction transaction)
        {
            var catItem = await _context.CategoryItems.Include(ci => ci.Category).FirstOrDefaultAsync(ci => ci.Id == transaction.CategoryItemId);
            if (ModelState.IsValid)
            {
                if(catItem.Category.Name == "Deposits")
                {
                    transaction.TransactionType = TransactionType.Deposit;
                }
                else
                {
                    transaction.TransactionType = TransactionType.Withdrawal;
                }
                var bankAccount = await _context.BankAccounts
                    .Include(ba => ba.HouseHold)
                    .FirstOrDefaultAsync(ba => ba.Id == transaction.BankAccountId);
                if(transaction.TransactionType == TransactionType.Deposit)
                {
                    bankAccount.CurrentBalance = bankAccount.CurrentBalance + transaction.Amount;
                }
                else
                {
                    bankAccount.CurrentBalance = bankAccount.CurrentBalance - transaction.Amount;
                }
                var categoryItem = await _context.CategoryItems.FirstOrDefaultAsync(ci => ci.Id == transaction.CategoryItemId);
                categoryItem.ActualAmount = categoryItem.ActualAmount + transaction.Amount;
                transaction.FpUserId = _userManager.GetUserId(User);
                transaction.Created = DateTime.Now;
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                await _notifcationService.NotifyAsync(transaction, bankAccount);
                return RedirectToAction("Details", "HouseHolds", new { id = bankAccount.HouseHoldId});
            }
            TempData["Error"] = "Error creating your transaction";
            return RedirectToAction("Index", "Home");
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["BankAccountId"] = new SelectList(_context.BankAccounts, "Id", "Name", transaction.BankAccountId);
            ViewData["CategoryItemId"] = new SelectList(_context.CategoryItems, "Id", "Description", transaction.CategoryItemId);
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "Id", transaction.FpUserId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BankAccountId,CategoryItemId,FpUserId,Created,TransactionType,Memo,Amount,IsDeleted")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            ViewData["BankAccountId"] = new SelectList(_context.BankAccounts, "Id", "Name", transaction.BankAccountId);
            ViewData["CategoryItemId"] = new SelectList(_context.CategoryItems, "Id", "Description", transaction.CategoryItemId);
            ViewData["FpUserId"] = new SelectList(_context.Users, "Id", "Id", transaction.FpUserId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.BankAccount)
                .Include(t => t.CategoryItem)
                .Include(t => t.FpUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.BankAccount)
                .Include(t => t.CategoryItem)
                .FirstOrDefaultAsync(t => t.Id == id);
            transaction.IsDeleted = true;

            if(transaction.TransactionType == TransactionType.Deposit)
            {
                transaction.BankAccount.CurrentBalance -= transaction.Amount;
            }
            else
            {
                transaction.BankAccount.CurrentBalance += transaction.Amount;
            }

            if(transaction.CategoryItem != null)
            {
                transaction.CategoryItem.ActualAmount -= transaction.Amount;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
