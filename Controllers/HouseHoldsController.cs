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
using Microsoft.AspNetCore.Authorization;

namespace FinancialPortalProject.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Details(int? id, int? categoryId)
        {
            if (id == null)
            {
                return NotFound();
            }
            var vm = new HouseHoldsDetailsVM();
            vm.Household = await _context.HouseHolds
                 .Include(hh => hh.Members)
                 .FirstOrDefaultAsync(hh => hh.Id == id);
            if (vm.Household == null)
            {
                return NotFound();
            }
            vm.BankAccounts = await _context.BankAccounts
                .Where(ba => ba.HouseHoldId == vm.Household.Id && ba.IsDeleted == false).ToListAsync();
            vm.Categories = await _context.Categories
                .Include(c => c.CategoryItems)
                .Where(c => c.HouseHoldId == vm.Household.Id && c.IsDeleted == false).ToListAsync();
            vm.HhTransactions = _context.Transactions
                .Include(t => t.CategoryItem).ThenInclude(ci => ci.Category)
                .Include(t => t.BankAccount)
                .Include(t => t.FpUser)
                .Where(t => t.BankAccount.HouseHoldId == vm.Household.Id && t.IsDeleted == false).ToList();
            foreach (var category in vm.Household.Categories)
            {
                foreach (var item in category.CategoryItems)
                {
                    category.AmountSpent += item.ActualAmount;
                }
            }
            var nonHeads = new List<FpUser>();
            foreach (var user in vm.Household.Members)
            {
                if(!await _userManager.IsInRoleAsync(user, nameof(Roles.Head)))
                {
                    nonHeads.Add(user);
                }
            }
            var itemCategories = vm.Categories.Where(c => c.CategoryItems.Count() > 0);
            var bankAccounts = await _context.BankAccounts.Where(ba => ba.HouseHoldId == vm.Household.Id && ba.IsDeleted == false).ToListAsync();
            ViewData["BankAccounts"] = new SelectList(bankAccounts, "Id", "Name");
            ViewData["AllCategories"] = new SelectList(vm.Categories, "Id", "Name");
            ViewData["NonHeads"] = new SelectList(nonHeads, "Id", "FullName");
            ViewData["Categories"] = new SelectList(itemCategories, "Id", "Name");
            if (categoryId != null)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.IsDeleted == false);
                var categoryItems = await _context.CategoryItems.Where(ci => ci.CategoryId == categoryId && ci.IsDeleted == false).ToListAsync();
                var selectedCategory = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
                if(categoryItems.Count() == 0)
                {
                    TempData["Alert"] = "Please select a category with at least one category item to make a transaction";
                }
                else
                {
                    ViewData["CategorySelect"] = new SelectList(categoryItems, "Id", "Name");
                    ViewData["Categories"] = new SelectList(itemCategories, "Id", "Name", selectedCategory.Id);
                    ViewData["CategoryName"] = $"Transaction for {category.Name}";
                }              
            }
            var userId = _userManager.GetUserId(User);
            if (User.IsInRole(nameof(Roles.Head)) && _context.Notifications.Where(n => n.HouseHoldId == vm.Household.Id && n.IsRead == false).Count() > 0)
            {
                vm.HasNotifcations = true;
            }
            else if (_context.Notifications.Where(n => n.FpUserId == userId && n.IsRead == false).Count() > 0)
            {
                vm.HasNotifcations = true;
            }
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
        public async Task<IActionResult> Create([Bind("Name,Greeting")] HouseHold houseHold)
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

                var deposits = new Category
                {
                    HouseHoldId = houseHold.Id,
                    Name = "Deposits",
                    Description = "Deposits",
                };
                _context.Add(deposits);
                await _context.SaveChangesAsync();
                var regular = new CategoryItem
                {
                    CategoryId = deposits.Id,
                    Name = "Regular Income",
                    Description = "Regular Income",
                    TargetAmount = 0
                };
                var extra = new CategoryItem
                {
                    CategoryId = deposits.Id,
                    Name = "Extra Income",
                    Description = "Extra Income",
                    TargetAmount = 0
                };
                _context.Add(regular);
                _context.Add(extra);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "HouseHolds", new { id = houseHold.Id });
            }
            return View(houseHold);
        }

        // GET: HouseHolds/Edit/5
        public async Task<IActionResult> Analytics()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.HouseHoldId == null)
            {
                TempData["Alert"] = "You must be in a household to view data";
                return RedirectToAction("Index", "Home");
            }
            var household = await _context.HouseHolds.FindAsync(user.HouseHoldId);
            return View(household);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveHousehold()
        {
            var user = await _userManager.GetUserAsync(User);
            var household = await _context.HouseHolds
                .Include(hh => hh.Members)
                .FirstOrDefaultAsync(hh => hh.Id == user.HouseHoldId);
            //var test = household.Members.Where(u => await _userManager.IsInRoleAsync(u, nameof(Roles.Head))).ToList();
            var heads = new List<FpUser>();
            foreach (var member in household.Members)
            {
                if (await _userManager.IsInRoleAsync(member, nameof(Roles.Head)))
                {
                    heads.Add(member);
                }
            }

            if (User.IsInRole(nameof(Roles.Head)) && household.Members.Count() > 1 && heads.Count() == 1)
            {
                TempData["RemoveMembers"] = "A head of household may only leave if they are the only person in the household, or " +
                    "if there is another head of household in place. Please promote another head of household or remove all other members.";
                return RedirectToAction("Details", "HouseHolds", new { id = household.Id });
            }

            user.HouseHoldId = null;
            await _context.SaveChangesAsync();
            if (household.Members.Count() == 0)
            {
                _context.HouseHolds.Remove(household);
                await _context.SaveChangesAsync();
            }
            foreach (var bankAccount in _context.BankAccounts.Where(ba => ba.OwnerId == user.Id))
            {
                bankAccount.IsDeleted = true;
            }
            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            await _userManager.RemoveFromRoleAsync(user, userRole);
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(string memberId)
        {
            var member = await _context.Users.FindAsync(memberId);
            member.HouseHoldId = null;
            await _userManager.RemoveFromRoleAsync(member, nameof(Roles.Member));
            var memberAccounts = await _context.BankAccounts.Where(ba => ba.OwnerId == member.Id).ToListAsync();
            foreach (var account in memberAccounts)
            {
                account.IsDeleted = true;
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{member.FullName} has been successfully removed from your household";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteMember(string memberId, bool stepDown)
        {
            var memeber = await _context.Users.FindAsync(memberId);
            await _userManager.RemoveFromRoleAsync(memeber, nameof(Roles.Member));
            await _userManager.AddToRoleAsync(memeber, nameof(Roles.Head));
            if(stepDown)
            {
                var user = await _userManager.GetUserAsync(User);
                await _userManager.RemoveFromRoleAsync(user, nameof(Roles.Head));
                await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
                await _context.SaveChangesAsync();
                await _signInManager.RefreshSignInAsync(user);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "HouseHolds", new { id = memeber.HouseHoldId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepDown()
        {
            var user = await _userManager.GetUserAsync(User);
            var heads = new List<FpUser>();
            foreach (var member in _context.Users.Where(u => u.HouseHoldId == user.HouseHoldId).ToList())
            {
                if (await _userManager.IsInRoleAsync(member, nameof(Roles.Head)))
                {
                    heads.Add(member);
                }
            }
            if(heads.Count() == 1)
            {
                TempData["Error"] = "A head of household can not step down unless there is another head of household in place";
                return RedirectToAction("Details", "HouseHolds", new { id = user.HouseHoldId });
            }          
            await _userManager.RemoveFromRoleAsync(user, nameof(Roles.Head));
            await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Details", "HouseHolds", new { id = user.HouseHoldId });
        }
    }
}
