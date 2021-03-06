﻿using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BragiBlogPoster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BragiBlogPoster.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<BlogUser> userManager;
        private readonly IEmailSender          emailSender;

        public ForgotPasswordModel(UserManager<BlogUser> userManager, IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if ( this.ModelState.IsValid)
            {
                BlogUser user = await this.userManager.FindByEmailAsync( this.Input.Email).ConfigureAwait( false );
                if (user == null || !(await this.userManager.IsEmailConfirmedAsync(user).ConfigureAwait( false ) ))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                string code = await this.userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait( false );
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string callbackUrl = this.Url.Page(
                                                   "/Account/ResetPassword",
                                                   pageHandler: null,
                                                   values: new { area = "Identity", code },
                                                   protocol: this.Request.Scheme);

                await this.emailSender.SendEmailAsync(
                                                      this.Input.Email,
                                                      "Reset Password",
                                                      $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait( false );

                return this.RedirectToPage("./ForgotPasswordConfirmation");
            }

            return this.Page();
        }
    }
}
