using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя пользователя.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Подтвердите пароль.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}
