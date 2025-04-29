using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class EmailConfirmationCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
