using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ishitagualti.Models;


namespace ishitagualti.Data;


public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db"); // Ensure this points to your SQLite database
    }
     public DbSet<MakeupBooking> MakeupBookings { get; set; }

    public DbSet<PdfLink> PdfLinks { get; set; } 


   
}


