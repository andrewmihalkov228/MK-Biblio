using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите имя пользователя.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите пароль.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
