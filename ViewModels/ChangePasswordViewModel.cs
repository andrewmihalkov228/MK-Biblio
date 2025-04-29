using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Введите новый пароль.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Новый пароль должен быть не менее 6 символов.")]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Подтвердите новый пароль.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и подтверждение не совпадают.")]
        [Display(Name = "Подтвердите новый пароль")]
        public string ConfirmPassword { get; set; }
    }
}
