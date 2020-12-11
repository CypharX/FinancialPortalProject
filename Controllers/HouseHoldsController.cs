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
using FinancialPortalProject.Models.ViewModels;

namespace FinancialPortalProject.Controllers
{
    public class HouseHoldsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FpUser> _userManager;
        private readonly SignInManager<FpUser> _signInManager;

        public HouseHoldsController(ApplicationDbContext context, UserManager<FpUser> userManager, SignInManager<FpUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: HouseHolds
        public async Task<IActionResult> Index()
        {
            return View(await _context.HouseHolds.ToListAsync());
        }

        // GET: HouseHolds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var vm = new HouseHoldsDetailsVM();
            vm.Household = await _context.HouseHolds
                 .Include(hh => hh.Members)
                 .Include(hh => hh.BankAccounts).ThenInclude(ba => ba.Transactions)
                 .Include(hh => hh.Categories).ThenInclude(c => c.CategoryItems)
                 .FirstOrDefaultAsync(m => m.Id == id);
            if (vm.Household == null)
            {
                return NotFound();
            }
            var hhCategories = await _context.Categories.Where(c => c.HouseHoldId == vm.Household.Id).ToListAsync();
            ViewData["Categories"] = new SelectList(hhCategories, "Id", "Name");
            vm.HhTransactions = _context.Transactions
                .Include(t => t.CategoryItem).ThenInclude(ci => ci.Category)
                .Include(t => t.BankAccount)
                .Include(t => t.FpUser)
                .Where(t => t.BankAccount.HouseHoldId == vm.Household.Id).ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int householdId, int categoryId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            var categoryItems = await _context.CategoryItems.Where(ci => ci.CategoryId == categoryId).ToListAsync();
            var vm = new HouseHoldsDetailsVM();
            vm.Household = await _context.HouseHolds
                 .Include(hh => hh.Members)
                 .Include(hh => hh.BankAccounts).ThenInclude(ba => ba.Transactions)
                 .Include(hh => hh.Categories).ThenInclude(c => c.CategoryItems)
                .FirstOrDefaultAsync(m => m.Id == householdId);
            var hhCategories = await _context.Categories.Where(c => c.HouseHoldId == vm.Household.Id).ToListAsync();
            var bankAccounts = await _context.BankAccounts.Where(ba => ba.HouseHoldId == vm.Household.Id).ToListAsync();
            var selectedCategory = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
            ViewData["CategorySelect"] = new SelectList(categoryItems, "Id", "Name");
            ViewData["Categories"] = new SelectList(hhCategories, "Id", "Name", selectedCategory.Id);
            ViewData["BankAccounts"] = new SelectList(bankAccounts, "Id", "Name");
            ViewData["CategoryName"] = $"Transaction for {category.Name}";
            vm.HhTransactions = _context.Transactions
                .Include(t => t.CategoryItem).ThenInclude(ci => ci.Category)
                .Include(t => t.BankAccount)
                .Include(t => t.FpUser)
                .Where(t => t.BankAccount.HouseHoldId == vm.Household.Id).ToList();
            return View(vm);
        }

        // GET: HouseHolds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HouseHolds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Greeting,Established")] HouseHold houseHold)
        {
            if (ModelState.IsValid)
            {
                houseHold.Established = DateTime.Now;
                _context.Add(houseHold);
                await _context.SaveChangesAsync();
                var user = await _userManager.GetUserAsync(User);
                user.HouseHoldId = houseHold.Id;
                houseHold.Members.Add(user);
                await _userManager.AddToRoleAsync(user, Roles.Head.ToString());
                await _context.SaveChangesAsync();
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Details", "HouseHolds", new { id = houseHold.Id });
            }
            return View(houseHold);
        }

        // GET: HouseHolds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var houseHold = await _context.HouseHolds.FindAsync(id);
            if (houseHold == null)
            {
                return NotFound();
            }
            return View(houseHold);
        }

        // POST: HouseHolds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Greeting,Established")] HouseHold houseHold)
        {
            if (id != houseHold.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(houseHold);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseHoldExists(houseHold.Id))
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
            return View(houseHold);
        }

        // GET: HouseHolds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var houseHold = await _context.HouseHolds
                .FirstOrDefaultAsync(m => m.Id == id);
            if (houseHold == null)
            {
                return NotFound();
            }

            return View(houseHold);
        }

        // POST: HouseHolds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var houseHold = await _context.HouseHolds.FindAsync(id);
            _context.HouseHolds.Remove(houseHold);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseHoldExists(int id)
        {
            return _context.HouseHolds.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember()
        {
            var user = await _userManager.GetUserAsync(User);
            var household = await _context.HouseHolds.Include(hh => hh.Members).FirstOrDefaultAsync(hh => hh.Id == user.HouseHoldId);

            if (User.IsInRole(nameof(Roles.Head)) && household.Members.Count() > 1)
            {
                TempData["RemoveMembers"] = "The head of household may only leave if they are the only person in the household.";
                return RedirectToAction("Details", "HouseHolds", new { id = household.Id });
            }

            user.HouseHoldId = null;
            await _context.SaveChangesAsync();
            if (household.Members.Count() == 0)
            {
                _context.HouseHolds.Remove(household);
                await _context.SaveChangesAsync();
            }
            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            await _userManager.RemoveFromRoleAsync(user, userRole);
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", "Home");
        }


    }
}
