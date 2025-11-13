using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Registration> Registrations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Users ----------
        modelBuilder.Entity<User>(u =>
        {
            u.ToTable("Users", t =>
            {
                t.HasCheckConstraint("CK_Users_NotEmpty",
                    "FullName <> '' AND UserName <> '' AND Email <> '' AND PasswordHash <> ''");
            });

            u.HasKey(x => x.Id);
            u.Property(x => x.FullName).IsRequired();
            u.Property(x => x.UserName).IsRequired();
            u.Property(x => x.Email).IsRequired();
            u.Property(x => x.PasswordHash).IsRequired();
            u.HasIndex(x => x.UserName).IsUnique();
        });

        // ---------- Events ----------
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Events", t =>
            {
                t.HasCheckConstraint("CK_Events_NotEmpty",
                    "Name <> '' AND Description <> '' AND Location <> ''");
            });

            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Location).IsRequired();
        });

        // ---------- Registrations ----------
        modelBuilder.Entity<Registration>(r =>
        {
            r.ToTable("Registrations", t =>
            {
                t.HasCheckConstraint("CK_Registrations_NotEmpty",
                    "Name <> '' AND Email <> '' AND PhoneNumber <> ''");
            });

            r.HasKey(x => x.Id);
            r.Property(x => x.Name).IsRequired();
            r.Property(x => x.Email).IsRequired();
            r.Property(x => x.PhoneNumber).IsRequired();
            r.Property(x => x.EventId).IsRequired();

            r.HasOne<Event>()
             .WithMany(e => e.Registrations)
             .HasForeignKey(x => x.EventId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
