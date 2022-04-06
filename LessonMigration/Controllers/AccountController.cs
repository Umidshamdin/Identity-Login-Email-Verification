using LessonMigration.Models;
using LessonMigration.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LessonMigration.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            AppUser newUser = new AppUser()
            {
                FullName = registerVM.FullName,
                UserName=registerVM.UserName,
                Email=registerVM.Email
            };
            
            IdentityResult result = await _userManager.CreateAsync(newUser, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                   
                }
                return View(registerVM);


            }


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            var link = Url.Action(nameof(VerifiEmail),"Account", new { userId = newUser, token = code },Request.Scheme,Request.Host.ToString());
            await SendEmail(newUser.Email,link);

            return RedirectToAction(nameof(EmailVerification));
        }

        public async Task<IActionResult> VerifiEmail(string userId,string token)
        {
            return Ok();
        }
        public IActionResult EmailVerification()
        {
            return View();
        }
        public async Task SendEmail(string emailAddress,string url)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("umidash@code.edu.az", "Umid");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress(emailAddress, "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = $"<a href ={Url}>Click here</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
    }
}
