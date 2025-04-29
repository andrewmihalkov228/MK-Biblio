using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Название книги обязательно для заполнения.")]
        [StringLength(255, ErrorMessage = "Название книги не должно превышать 255 символов.")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Автор обязателен для заполнения.")]
        [StringLength(255, ErrorMessage = "Имя автора не должно превышать 255 символов.")]
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Жанр обязателен для заполнения.")]
        [StringLength(100, ErrorMessage = "Название жанра не должно превышать 100 символов.")]
        [Display(Name = "Жанр")]
        public string Genre { get; set; }

        [StringLength(255, ErrorMessage = "Название издательства не должно превышать 255 символов.")]
        [Display(Name = "Издательство")]
        public string Publisher { get; set; }

        [Range(1000, 2025, ErrorMessage = "Год издания должен быть между 1000 и 2025.")]
        [Display(Name = "Год издания")]
        public int PublicationYear { get; set; }

        [StringLength(20, ErrorMessage = "ISBN не должен превышать 20 символов.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "ISBN может содержать только цифры и тире.")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Общее количество экземпляров обязательно.")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество экземпляров должно быть больше 0.")]
        [Display(Name = "Всего экземпляров")]
        public int TotalCopies { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Количество доступных экземпляров не может быть отрицательным.")]
        [Display(Name = "Доступно экземпляров")]
        public int AvailableCopies { get; set; }

        [Required(ErrorMessage = "Укажите стоимость книги/аренды.")]
        [Range(0.01, 10000.00, ErrorMessage = "Стоимость должна быть положительной.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Обложка")]
        public byte[] CoverImage { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; }
    }
}
