using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinancialPortalProject.Data;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using FinancialPortalProject.Enums;

namespace FinancialPortalProject.Controllers
{
    public class InvitationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<FpUser> _userManager;
        private readonly SignInManager<FpUser> _signInManager;

        public InvitationsController(ApplicationDbContext context, IEmailSender emailSender, UserManager<FpUser> userManager, SignInManager<FpUser> signInManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Invitations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invitations.Include(i => i.HouseHold);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invitations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invitation = await _context.Invitations
                .Include(i => i.HouseHold)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invitation == null)
            {
                return NotFound();
            }

            return View(invitation);
        }

        // GET: Invitations/Create
        public IActionResult Create()
        {
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name");
            return View();
        }

        // POST: Invitations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HouseHoldId,Created,Expires,Accepted,EmailTo,Subject,Body,Code")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                invitation.Code = Guid.NewGuid();
                invitation.Created = DateTime.Now;
                _context.Add(invitation);
                await _context.SaveChangesAsync();
                var callbackUrl = Url.Action("AcceptInvitation", "Invitations",
                                    new { email = invitation.EmailTo, code = invitation.Code }, protocol: Request.Scheme);
                var emailBody = $"{invitation.Body} <br /> Register and accept by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>" +
                                 $" or if already registered you can log in and use the following code <br /> Invitation Code: {invitation.Code}";
                await _emailSender.SendEmailAsync(invitation.EmailTo, invitation.Subject, emailBody);
                TempData["Success"] = $"Your invitation to {invitation.EmailTo} has been sent.";
                return RedirectToAction("Details", "HouseHolds", new { id = invitation.HouseHoldId});
            }
            TempData["Error"] = "Your invitation could not be sent";
            return View(invitation);
        }

        public async Task<IActionResult> AcceptInvitation(string email, string code)
        {
            var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Code.ToString() == code);
            if(invitation == null)
            {
                TempData["InviteFailed"] = "Your invitation could not be found";
                return RedirectToAction("InviteFailed", "Invitations");
            }
            if(DateTime.Now > invitation.Expires)
            {
                TempData["InviteFailed"] = $"Your invitation expired on {invitation.Expires:g}";
                return RedirectToAction("InviteFailed", "Invitations");
            }
            if(invitation.Accepted)
            {
                TempData["InviteFailed"] = "Your invitation has previously been accepted";
                return RedirectToAction("InviteFailed", "Invitations");
            }
            
            else
            {
                if(User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if(user.Email.ToLower() != invitation.EmailTo.ToLower())
                    {
                        TempData["InviteFailed"] = "Your invitation can not be accepted with your email address";
                        return RedirectToAction("InviteFailed", "Invitations");
                    }
                    if(user.HouseHoldId != null)
                    {
                        TempData["InviteFailed"] = "You may only be in one household at a time.";
                        return RedirectToAction("InviteFailed", "Invitations");
                    }
                    var household = await _context.HouseHolds.FirstOrDefaultAsync(hh => hh.Id == invitation.HouseHoldId);
                    invitation.Accepted = true;
                    household.Members.Add(user);
                    await _userManager.AddToRoleAsync(user, Roles.Member.ToString());
                    await _context.SaveChangesAsync();
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("Details", "Households", new { id = household.Id });
                }
                return RedirectToPage("/Account/Register", new { area = "Identity", email = invitation.EmailTo, code = invitation.Code });
            }
        }

        public IActionResult InviteFailed()
        {
            return View();
        }

        // GET: Invitations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", invitation.HouseHoldId);
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HouseHoldId,Created,Expires,Accepted,EmailTo,Subject,Body,Code")] Invitation invitation)
        {
            if (id != invitation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invitation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvitationExists(invitation.Id))
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
            ViewData["HouseHoldId"] = new SelectList(_context.HouseHolds, "Id", "Name", invitation.HouseHoldId);
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invitation = await _context.Invitations
                .Include(i => i.HouseHold)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invitation == null)
            {
                return NotFound();
            }

            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invitation = await _context.Invitations.FindAsync(id);
            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvitationExists(int id)
        {
            return _context.Invitations.Any(e => e.Id == id);
        }
    }
}
