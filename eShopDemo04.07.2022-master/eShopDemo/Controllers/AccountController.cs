using eShopDemo.Models;
using eShopDemo.Services.Interfaces;
using eShopDemo.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace eShopDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ISendEmail _sendEmail;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ISendEmail sendEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendEmail = sendEmail;
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
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                UserName = registerVM.Username,
                Email = registerVM.Email
            };

            var result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(registerVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            var link = Url.Action(nameof(VerifyEmail), "Account", new { userId = newUser.Id, token = token }, Request.Scheme, Request.Host.ToString());

            await _sendEmail.SendEmail(newUser.Email, link);
            //await _signInManager.SignInAsync(newUser, false);

            return RedirectToAction(nameof(EmailVerification));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index),"Home");
        }

        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);

            if (user == null) user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
            
            if(user == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View();
            }
            if (!user.IsActivated)
            {
                ModelState.AddModelError("", "Your account has not been verified yet. Please, wait for the verification or contact the support.");
                return View();
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginVM.Password,false, false);

            if (signInResult.IsNotAllowed)
            {
                ModelState.AddModelError("", "Please, confirm your account.");
                return View();
            }
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View();
            }

            //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
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult EmailVerification()
        {
            return View();
        }
        
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            if (userId == null || token == null) return BadRequest();
            AppUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();

            await _userManager.ConfirmEmailAsync(user, token);
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
