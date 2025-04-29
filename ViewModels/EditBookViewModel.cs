using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class EditBookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Название книги обязательно для заполнения.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Автор обязателен для заполнения.")]
        public string Author { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Жанр обязателен для заполнения.")]
        public string Genre { get; set; }
        public string Publisher { get; set; }

        [Range(1000, 3000, ErrorMessage = "Год издания должен быть между 1000 и 2025.")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "ISBN обязателен для заполнения.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "ISBN может содержать только цифры и тире.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Укажите общее количество экземпляров.")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество экземпляров должно быть больше 0.")]
        public int TotalCopies { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }

        public IFormFile? CoverImageFile { get; set; }

        public byte[] ExistingCoverImagePath { get; set; }
    }
}
