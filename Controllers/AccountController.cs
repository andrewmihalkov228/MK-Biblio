using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace LibrarySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
        }

        private string GenerateConfirmationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        [AllowAnonymous]
        public IActionResult Login(bool confirmationSuccess = false)
        {
            if (confirmationSuccess)
            {
                ViewData["SuccessMessage"] = "Email успешно подтвержден. Теперь вы можете войти.";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user.IsBanned)
                {
                    if (user.BanEndDate.HasValue && user.BanEndDate.Value < DateTime.UtcNow)
                    {
                        user.IsBanned = false;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ваш аккаунт заблокирован из-за нарушения правил библиотеки.");
                        return View(model);
                    }
                }

                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user) && await _userManager.IsInRoleAsync(user, "user"))
                    {
                        ModelState.AddModelError(string.Empty, "Пожалуйста, подтвердите ваш email перед входом.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (await _userManager.IsInRoleAsync(user, "librarian"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("", "Аккаунт временно заблокирован. Попробуйте позже.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");

                    var confirmationCode = GenerateConfirmationCode();

                    var existingCodes = _context.EmailConfirmationCodes.Where(c => c.UserId == user.Id);
                    _context.EmailConfirmationCodes.RemoveRange(existingCodes);

                    _context.EmailConfirmationCodes.Add(new EmailConfirmationCode
                    {
                        UserId = user.Id,
                        Code = confirmationCode
                    });
                    await _context.SaveChangesAsync();

                    await _emailSender.SendEmailAsync(model.Email, "Код подтверждения email - МК Библио",
                        $"Ваш код подтверждения для МК Библио: <strong>{confirmationCode}</strong>.");

                    return RedirectToAction("EnterConfirmationCode", "Account", new { email = model.Email });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult EnterConfirmationCode(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }
            var model = new EnterConfirmationCodeViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterConfirmationCode(EnterConfirmationCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким email не найден.");
                return View(model);
            }

            var storedCode = await _context.EmailConfirmationCodes
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.Code == model.Code);

            if (storedCode == null)
            {
                ModelState.AddModelError("Code", "Неверный код подтверждения.");
                return View(model);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _context.EmailConfirmationCodes.Remove(storedCode);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", new { confirmationSuccess = true });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Произошла ошибка при подтверждении email.");
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ConfirmEmailAsync error: {error.Description}");
                }
                return View(model);
            }
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var rentals = await _context.Rentals
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();

            var viewModel = new UserProfileViewModel { Rentals = rentals };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int rentalId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var rental = await _context.Rentals
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);

            if (rental == null)
            {
                return NotFound("Указанная аренда не найдена.");
            }

            if (rental.UserId != user.Id)
            {
                return NotFound("Аренда не найдена или не принадлежит вам.");
            }

            if (rental.IssuedDate == null)
            {
                TempData["ErrorMessage"] = "Книга еще не была вам выдана библиотекой.";
                return RedirectToAction("Profile");
            }

            if (rental.ReturnRequestDate != null || rental.ActualReturnDate != null)
            {
                TempData["ErrorMessage"] = "Возврат этой книги уже запрошен или она фактически возвращена.";
                return RedirectToAction("Profile");
            }

            rental.ReturnRequestDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Запрос на возврат книги отправлен. Пожалуйста, верните книгу библиотекарю.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Пароль успешно изменен. Пожалуйста, войдите снова.";
            return RedirectToAction("Login");
        }
    }
}
