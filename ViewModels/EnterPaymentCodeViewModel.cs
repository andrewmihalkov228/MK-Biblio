﻿using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class EnterPaymentCodeViewModel
    {
        [Required]
        public int BookId { get; set; }

        public string BookTitle { get; set; }
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Введите код подтверждения.")]
        [Display(Name = "Код подтверждения")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен состоять из 6 цифр.")]
        public string Code { get; set; }
    }
}
