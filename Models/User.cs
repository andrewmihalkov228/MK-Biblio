using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<Rental> Rentals { get; set; }

        public bool IsBanned { get; set; } = false;
        public DateTime? BanEndDate { get; set; }
    }
}
