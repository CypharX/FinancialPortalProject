using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FinancialPortalProject.Enums;

namespace FinancialPortalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<FpUser> _userManager;
        private readonly SignInManager<FpUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<FpUser> userManager, SignInManager<FpUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {      
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if(user.HouseHoldId != null)
                {
                    return RedirectToAction("Details", "HouseHolds", new { id = user.HouseHoldId });
                }
                return RedirectToAction("Lobby");
            }
            return View();
        }

        [Authorize]
        public IActionResult Lobby()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DemoLogin()
        {
            var users = await _userManager.GetUsersInRoleAsync(nameof(Roles.Demo));
            var user = users.FirstOrDefault();
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
