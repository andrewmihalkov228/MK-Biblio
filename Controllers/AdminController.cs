using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "admin, librarian")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.ActualReturnDate == null)
                .OrderBy(r => r.ReturnRequestDate != null ? 0 : 1)
                .ThenBy(r => r.IssuedDate != null ? 0 : 1)
                .ThenBy(r => r.RentalDate)
                .ToListAsync();

            var viewModel = new AdminViewModel
            {
                Rentals = rentals
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddBook(AdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rentals = await _context.Rentals.Include(r => r.Book).Include(r => r.User).Where(r => r.ActualReturnDate == null).ToListAsync();
                return View("Index", model);
            }

            if (model.NewBook.CoverImageFile == null || model.NewBook.CoverImageFile.Length == 0)
            {
                ModelState.AddModelError("NewBook.CoverImageFile", "Необходимо выбрать изображение обложки.");
                model.Rentals = await _context.Rentals.Include(r => r.Book).Include(r => r.User).Where(r => r.ActualReturnDate == null).ToListAsync();
                return View("Index", model);
            }

            if (await _context.Books.AnyAsync(b => b.ISBN == model.NewBook.ISBN))
            {
                ModelState.AddModelError("NewBook.ISBN", "Книга с таким ISBN уже существует.");
                model.Rentals = await _context.Rentals.Include(r => r.Book).Include(r => r.User).Where(r => r.ActualReturnDate == null).ToListAsync();
                return View("Index", model);
            }

            byte[] imageData = null;

            using (var binaryReader = new BinaryReader(model.NewBook.CoverImageFile.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)model.NewBook.CoverImageFile.Length);
            }

            var book = new Book
            {
                Title = model.NewBook.Title,
                Author = model.NewBook.Author,
                Description = model.NewBook.Description,
                Genre = model.NewBook.Genre,
                Publisher = model.NewBook.Publisher,
                PublicationYear = model.NewBook.PublicationYear,
                ISBN = model.NewBook.ISBN,
                TotalCopies = model.NewBook.TotalCopies,
                AvailableCopies = model.NewBook.TotalCopies,
                CoverImage = imageData,
                Price = model.NewBook.Price
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin,librarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseBooking(int rentalId)
        {
            var rental = await _context.Rentals.Include(r => r.Book).FirstOrDefaultAsync(r => r.RentalId == rentalId);
            if (rental == null)
            {
                return NotFound();
            }

            rental.Book.AvailableCopies++;

            rental.ActualReturnDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin,librarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBook(int rentalId)
        {
            var rental = await _context.Rentals.Include(r => r.Book).FirstOrDefaultAsync(r => r.RentalId == rentalId);

            if (rental == null)
            {
                return NotFound();
            }

            if (rental.IssuedDate != null)
            {
                TempData["ErrorMessage"] = "Книга уже выдана.";
                return RedirectToAction(nameof(Index));
            }

            if (rental.Book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "Нет доступных экземпляров для выдачи.";
                return RedirectToAction(nameof(Index));
            }

            rental.IssuedDate = DateTime.Now;
            rental.DueDate = rental.IssuedDate.Value.AddDays(7);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
