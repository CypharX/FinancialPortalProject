using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity;

namespace FinancialPortalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<FpUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<FpUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if(User.Identity.IsAuthenticated)
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

        public IActionResult Lobby()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
