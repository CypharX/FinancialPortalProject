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
            ApplicationDbContext context)
        {
            await SeedRolesAsync(roleManager);
           
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Head.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Member.ToString()));           
        }

        //private static async Task SeedDefaultAdminAsync(UserManager<FpUser> userManager)
        //{
        //    var adminSettings = _adminSettings.Value;

        //    if (await userManager.FindByEmailAsync(adminSettings.Email) == null)
        //    {
        //        var defaultAdmin = new FpUser
        //        {
        //            FirstName = adminSettings.FirstName,
        //            LastName = adminSettings.LastName,
        //            Email = adminSettings.Email,
        //            UserName = adminSettings.Email,
        //            ImageName = adminSettings.ImageName,
        //            ImageData = await _fileService.AssignDefaultAvatarAsync(adminSettings.ImageName)
        //        };
        //        await userManager.CreateAsync(defaultAdmin, adminSettings.Password);
        //        await userManager.AddToRoleAsync(defaultAdmin, Roles.Admin.ToString());
        //    }        
        //}
    }
}
