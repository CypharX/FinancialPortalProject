using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using FinancialPortalProject.Services;
using FinancialPortalProject.Extensions;
using Microsoft.Extensions.Configuration;
using FinancialPortalProject.Data;
using FinancialPortalProject.Enums;

namespace FinancialPortalProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<FpUser> _signInManager;
        private readonly UserManager<FpUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IFP_FileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<FpUser> userManager,
            SignInManager<FpUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IFP_FileService fileService,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _fileService = fileService;
            _configuration = configuration;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            [NotMapped]
            [DataType(DataType.Upload)]
            [MaxFileSize(2 * 1024 * 1024)]
            [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
            public IFormFile Avatar { get; set; }

            public string Code { get; set; }
        }

        public async Task OnGetAsync(string email, string code, string returnUrl = null)
        {
            Email = email;
            Code = code;
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                var user = new FpUser
                { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName
                };
                if (Input.Avatar != null)
                {
                    user.ImageName = Input.Avatar.FileName;
                    user.ImageData = await _fileService.ConvertFileToByteArrayAsync(Input.Avatar);
                }
                //else
                //{
                //    var image = _configuration.GetSection("AdminSettings")["ImageName"];
                //    user.ImageData = await _fileService.AssignDefaultAvatarAsync(image);
                //    user.ImageName = image;
                //}
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded && string.IsNullOrEmpty(Input.Code))
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                else if (result.Succeeded && !string.IsNullOrEmpty(Input.Code))
                {
                    user.EmailConfirmed = true;
                    var invitation = _context.Invitations.FirstOrDefault(i => i.Code.ToString() == Input.Code);
                    invitation.Accepted = true;
                    var household = _context.HouseHolds.FirstOrDefault(hh => hh.Id == invitation.HouseHoldId);
                    household.Members.Add(user);
                    await _userManager.AddToRoleAsync(user, Roles.Member.ToString());
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Details", "HouseHolds", new { id = household.Id });
                }

                    foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
