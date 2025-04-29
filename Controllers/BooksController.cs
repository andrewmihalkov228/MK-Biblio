using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarySystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public BooksController(ApplicationDbContext context, IEmailSender emailSender, UserManager<User> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager; 
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "Нет доступных экземпляров книги для покупки/аренды.";
                return RedirectToAction(nameof(Details), new { id = book.BookId });
            }

            var userId = _userManager.GetUserId(User);

            var existingActiveRental = await _context.Rentals
               .FirstOrDefaultAsync(r => r.BookId == id &&
                                           r.UserId == userId &&
                                           r.ActualReturnDate == null);

            if (existingActiveRental != null)
            {
                TempData["ErrorMessage"] = "Вы уже арендовали эту книгу или она у вас на руках.";
                return RedirectToAction(nameof(Details), new { id = book.BookId });
            }

            return RedirectToAction(nameof(Payment), new { bookId = id });
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new EditBookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                ExistingCoverImagePath = book.CoverImage
            };

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Payment(int bookId)
        {
            var viewModel = new PaymentViewModel { BookId = bookId };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Books.FindAsync(model.BookId);

            if (book == null || user == null)
            {
                return NotFound("Книга или пользователь не найдены.");
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "Извините, нет доступных экземпляров для аренды.";
                return RedirectToAction(nameof(Details), new { id = book.BookId });
            }

            var paymentCode = GenerateConfirmationCode();

            var existingCodes = _context.PaymentConfirmationCodes
                                       .Where(c => c.UserId == user.Id && c.BookId == book.BookId);
            _context.PaymentConfirmationCodes.RemoveRange(existingCodes);

            _context.PaymentConfirmationCodes.Add(new PaymentConfirmationCode
            {
                UserId = user.Id,
                BookId = book.BookId,
                Code = paymentCode
            });
            await _context.SaveChangesAsync();

            var subject = "Код подтверждения аренды - МК Библио";
            var message = $@"
            <h1>Подтверждение аренды</h1>
            <p>Здравствуйте, {user.UserName}!</p>
            <p>Для завершения аренды книги <strong>'{book.Title}'</strong> введите следующий код подтверждения на сайте:</p>
            <h2>{paymentCode}</h2>
            <p>Стоимость аренды: {book.Price.ToString("C", CultureInfo.GetCultureInfo("ru-RU"))}.</p>
            <hr>
            <p>С уважением,<br>Команда МК Библио</p>";

            try
            {
                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"!!! Ошибка отправки email кода подтверждения оплаты: {ex.Message}");
                TempData["WarningMessage"] = "Не удалось отправить email с кодом подтверждения. Попробуйте позже.";
                return RedirectToAction(nameof(Details), new { id = book.BookId });
            }

            return RedirectToAction(nameof(EnterPaymentCode), new { bookId = book.BookId });
        }

        [Authorize]
        public async Task<IActionResult> EnterPaymentCode(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            var user = await _userManager.GetUserAsync(User);

            if (book == null || user == null)
            {
                return NotFound();
            }

            var model = new EnterPaymentCodeViewModel
            {
                BookId = bookId,
                BookTitle = book.Title,
                UserEmail = user.Email
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterPaymentCode(EnterPaymentCodeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var book = await _context.Books.FindAsync(model.BookId);

            if (user == null || book == null)
            {
                return NotFound();
            }

            var storedCode = await _context.PaymentConfirmationCodes
                .FirstOrDefaultAsync(c => c.UserId == user.Id &&
                                          c.BookId == model.BookId &&
                                          c.Code == model.Code);

            if (storedCode == null)
            {
                ModelState.AddModelError("Code", "Неверный код подтверждения.");
                model.BookTitle = book.Title;
                model.UserEmail = user.Email;
                return View(model);
            }

            await _context.Entry(book).ReloadAsync();

            var rental = new Rental
            {
                BookId = model.BookId,
                UserId = user.Id,
                RentalDate = DateTime.Now
            };

            book.AvailableCopies--;
            _context.Rentals.Add(rental);
            _context.PaymentConfirmationCodes.Remove(storedCode);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Аренда книги '{book.Title}' успешно подтверждена! Ожидайте выдачи книги в библиотеке.";
            return RedirectToAction("Profile", "Account");
        }

        private string GenerateConfirmationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBookViewModel model)
        {
            if (id != model.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.AvailableCopies > model.TotalCopies)
                {
                    ModelState.AddModelError("AvailableCopies", "Количество доступных экземпляров не может быть больше общего количества.");
                    return View(model);
                }

                try
                {
                    var book = await _context.Books.FindAsync(id);
                    if (book == null)
                    {
                        return NotFound();
                    }

                    book.Title = model.Title;
                    book.Author = model.Author;
                    book.Description = model.Description;
                    book.Genre = model.Genre;
                    book.Publisher = model.Publisher;
                    book.PublicationYear = model.PublicationYear;
                    book.ISBN = model.ISBN;
                    book.TotalCopies = model.TotalCopies;
                    book.AvailableCopies = model.AvailableCopies;

                    if (model.CoverImageFile != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.CoverImageFile.CopyToAsync(memoryStream);
                            book.CoverImage = memoryStream.ToArray();
                        }
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(model.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = model.BookId });
            }
            return View(model);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            var bookings = await _context.Rentals.Where(b => b.BookId == id).ToListAsync();
            _context.Rentals.RemoveRange(bookings);

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
