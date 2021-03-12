using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinancialPortalProject.Enums;
using FinancialPortalProject.Extensions;
using FinancialPortalProject.Models;
using FinancialPortalProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinancialPortalProject.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<FpUser> _userManager;
        private readonly SignInManager<FpUser> _signInManager;
        private readonly IFP_FileService _fileService;

        public IndexModel(
            UserManager<FpUser> userManager,
            SignInManager<FpUser> signInManager,
            IFP_FileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {    
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string OldPassword { get; set; }

            [NotMapped]
            [DataType(DataType.Upload)]
            [MaxFileSize(2 * 1024 * 1024)]
            [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
            public IFormFile Avatar { get; set; }
            public string ImageName { get; set; }
            public byte[] ImageData { get; set; }
        }

        private async Task LoadAsync(FpUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ImageData = user.ImageData,
                ImageName = user.ImageName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            user.Extension = Path.GetExtension(user.ImageName); 
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.IsInRole(nameof(Roles.Demo)))
            {
                TempData["Alert"] = "That action can not be done by demo users";
                return RedirectToAction("Index", "Home");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

                       
            if(Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    StatusMessage = "Error when trying update first name.";
                    return RedirectToPage();
                }
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    StatusMessage = "Error when trying update last name.";
                    return RedirectToPage();
                }
            }

            if (Input.Email != user.Email)
            {
                user.Email = Input.Email;
                user.UserName = Input.Email;
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    StatusMessage = "Error when trying update email.";
                    return RedirectToPage();
                }
            }

            if (Input.Avatar != null)
            {
                user.ImageData = await _fileService.ConvertFileToByteArrayAsync(Input.Avatar);
                user.ImageName = Input.Avatar.FileName;
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    StatusMessage = "Error when trying update avatar.";
                    return RedirectToPage();
                }
            }

            if (Input.Password != null && Input.OldPassword != null)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.Password);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
