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
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FpUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<FpUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var vm = new NotificationsIndexVm();
            var user = await _userManager.GetUserAsync(User);
            if(user.HouseHoldId == null)
            {
                TempData["Warning"] = "You must be in a household to receive notifications";
                return RedirectToAction("Index", "Home");
            }
            if(User.IsInRole(nameof(Roles.Head)))
            {
                vm.NewNotifications = await _context.Notifications
                    .Include(n => n.FpUser)
                    .Where(n => n.HouseHoldId == user.HouseHoldId && n.IsRead == false).ToListAsync();
                vm.OldNotifications = await _context.Notifications
                    .Include(n => n.FpUser)
                    .Where(n => n.HouseHoldId == user.HouseHoldId && n.IsRead == true).ToListAsync();
            }
            else
            {
                vm.NewNotifications = await _context.Notifications
                    .Include(n => n.FpUser)
                    .Where(n => n.FpUserId == user.Id && n.IsRead == false).ToListAsync();
                vm.OldNotifications = await _context.Notifications
                   .Include(n => n.FpUser)
                   .Where(n => n.FpUserId == user.Id && n.IsRead == true).ToListAsync();
            }
            vm.Household = await _context.HouseHolds.FindAsync(user.HouseHoldId);
            return View(vm);
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.HouseHold)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HouseHoldId,Created,Subject,Body,IsRead")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", notification.HouseHoldId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HouseHoldId,Created,Subject,Body,IsRead")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
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
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", notification.HouseHoldId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.HouseHold)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            TempData["Error"] = "Your notification has been deleted";
            return RedirectToAction("Index", "Notifications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Notifications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkUnread(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            notification.IsRead = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Notifications");
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = new List<Notification>();
            if(User.IsInRole(nameof(Roles.Head)))
            {
                notifications = await _context.Notifications.Where(n => n.HouseHoldId == user.HouseHoldId).ToListAsync();
            }
            else
            {
                notifications = await _context.Notifications.Where(n => n.FpUserId == user.Id).ToListAsync();
            }
            foreach (var notificaiton in notifications)
            {
                notificaiton.IsRead = true;
            }
            await _context.SaveChangesAsync();
            TempData["Alert"] = "All your notifications have been marked as read!";
            return RedirectToAction("Details", "HouseHolds", new { id = user.HouseHoldId });
        }
    }
}
