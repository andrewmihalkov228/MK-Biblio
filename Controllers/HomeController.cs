using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibrarySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(BookSearchViewModel searchModel)
        {
            IQueryable<Book> booksQuery = _context.Books;

            if (!string.IsNullOrEmpty(searchModel.SearchTerm) && !string.IsNullOrEmpty(searchModel.SearchField))
            {
                switch (searchModel.SearchField)
                {
                    case "Title":
                        booksQuery = booksQuery.Where(b => b.Title.Contains(searchModel.SearchTerm));
                        break;
                    case "Author":
                        booksQuery = booksQuery.Where(b => b.Author.Contains(searchModel.SearchTerm));
                        break;
                    case "Genre":
                        booksQuery = booksQuery.Where(b => b.Genre.Contains(searchModel.SearchTerm));
                        break;
                    case "ISBN":
                        booksQuery = booksQuery.Where(b => b.ISBN.Contains(searchModel.SearchTerm));
                        break;
                    case "PublicationYear":
                        if (int.TryParse(searchModel.SearchTerm, out int year))
                        {
                            booksQuery = booksQuery.Where(b => b.PublicationYear == year);
                        }
                        break;
                }
            }

            var books = await booksQuery.ToListAsync();
            return View(books);
        }

        public IActionResult Rules()
        {
            return View();
        }
    }
}
