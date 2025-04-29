using LibrarySystem.Models;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class AdminViewModel
    {
        public List<Rental> Rentals { get; set; } = new List<Rental>();
        public NewBookViewModel NewBook { get; set; } = new NewBookViewModel();
    }

    public class NewBookViewModel
    {
        [Required(ErrorMessage = "Название книги обязательно для заполнения.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Автор обязателен для заполнения.")]
        public string Author { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Жанр обязателен для заполнения.")]
        public string Genre { get; set; }
        public string Publisher { get; set; }

        [Range(1000, 2025, ErrorMessage = "Год издания должен быть между 1000 и 2025.")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "ISBN обязателен для заполнения.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "ISBN может содержать только цифры и тире.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Укажите общее количество экземпляров.")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество экземпляров должно быть больше 0.")]
        public int TotalCopies { get; set; }


        [Required(ErrorMessage = "Выберите файл обложки.")]
        public IFormFile CoverImageFile { get; set; }

        [Required(ErrorMessage = "Укажите стоимость книги.")]
        [Range(0.01, 10000.00, ErrorMessage = "Стоимость должна быть положительной.")]
        public decimal Price { get; set; }
    }
}
