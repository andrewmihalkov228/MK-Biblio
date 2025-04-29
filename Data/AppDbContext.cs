using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LibrarySystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<EmailConfirmationCode> EmailConfirmationCodes { get; set; }
        public DbSet<PaymentConfirmationCode> PaymentConfirmationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Book)
                .WithMany(book => book.Rentals)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.User)
                .WithMany(user => user.Rentals)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();
        }
    }
}
