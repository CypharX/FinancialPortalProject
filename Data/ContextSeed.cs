using FinancialPortalProject.Enums;
using FinancialPortalProject.Models;
using FinancialPortalProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Data
{
    public class ContextSeed
    {
        //private readonly IFP_FileService _fileService;
        //private readonly IOptions<AdminSettings> _adminSettings;

        //public ContextSeed(IOptions<AdminSettings> adminSettings)
        //{
        //    _adminSettings = adminSettings;
        //}
        

        public static async Task RunSeedMethodsAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<FpUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IFP_FileService fileService)
        {
            await SeedRolesAsync(roleManager);
            await SeedDefaultAdminAsync(userManager, configuration, fileService);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Head.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Member.ToString()));           
        }

        private static async Task SeedDefaultAdminAsync(UserManager<FpUser> userManager, IConfiguration configuration, IFP_FileService fileService)
        {

            var user = userManager.FindByEmailAsync(configuration.GetSection("AdminSettings")["Email"]).Result;
            if (user == null)
            {
                var defaultAdmin = new FpUser
                {
                    FirstName = configuration.GetSection("AdminSettings")["FirstName"],
                    LastName = configuration.GetSection("AdminSettings")["LastName"],
                    Email = configuration.GetSection("AdminSettings")["Email"],
                    UserName = configuration.GetSection("AdminSettings")["Email"],
                    ImageName = configuration.GetSection("AdminSettings")["ImageName"],
                    ImageData = await fileService.AssignDefaultAvatarAsync(configuration.GetSection("AdminSettings")["ImageName"]),
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(defaultAdmin, configuration.GetSection("AdminSettings")["Password"]);
                await userManager.AddToRoleAsync(defaultAdmin, Roles.Admin.ToString());
            }
        }
    }
}
