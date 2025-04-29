using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class PaymentViewModel
    {
        public int BookId { get; set; } // Скрытое поле

        [Required(ErrorMessage = "Введите номер карты.")]
        [Display(Name = "Номер карты")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Введите срок действия.")]
        [Display(Name = "Срок действия (ММ/ГГ)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "Введите CVV.")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; }
    }
}
