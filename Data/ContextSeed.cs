using FinancialPortalProject.Enums;
using FinancialPortalProject.Models;
using FinancialPortalProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Data
{
    public class ContextSeed
    {
       
        public static async Task RunSeedMethodsAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<FpUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IFP_FileService fileService)
        {
            await SeedRolesAsync(roleManager);
            await SeedDefaultAdminAsync(userManager, configuration, fileService);
            await SeedDemoUserAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Head.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Member.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Demo.ToString()));
        }

        private static async Task SeedDefaultAdminAsync(UserManager<FpUser> userManager, IConfiguration configuration, IFP_FileService fileService)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(configuration.GetSection("AdminSettings")["Email"]);
                if (user == null)
                {
                    var defaultAdmin = new FpUser
                    {
                        FirstName = configuration.GetSection("AdminSettings")["FirstName"],
                        LastName = configuration.GetSection("AdminSettings")["LastName"],
                        Email = configuration.GetSection("AdminSettings")["Email"],
                        UserName = configuration.GetSection("AdminSettings")["Email"],                       
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(defaultAdmin, configuration.GetSection("AdminSettings")["Password"]);
                    await userManager.AddToRoleAsync(defaultAdmin, Roles.Admin.ToString());
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("************** ERROR **************");
                Debug.WriteLine("Error Seeding Default Admin User.");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("***********************************");
                throw;
            }
          
        }

        private static async Task SeedDemoUserAsync(UserManager<FpUser> userManager)
        {
            try
            {
                var email = "demo@mailinator.com";
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    var demoUser = new FpUser
                    {
                        FirstName = "Daniel",
                        LastName = "Ryder",
                        Email = email,
                        UserName = email,                      
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(demoUser, "Abc&123");
                    await userManager.AddToRoleAsync(demoUser, Roles.Demo.ToString());
                    await userManager.AddToRoleAsync(demoUser, Roles.Head.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("************** ERROR **************");
                Debug.WriteLine("Error Seeding Demo User.");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("***********************************");
                throw;
            }

        }
    }
}
