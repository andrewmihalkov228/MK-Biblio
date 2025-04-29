using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Rental
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RentalId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime RentalDate { get; set; }

        public DateTime? IssuedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnRequestDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
