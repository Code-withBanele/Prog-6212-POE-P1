using Route = RaceDay.Models.Route;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options)
        : base(options)
    {
    }

    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organiser>()
            .ToTable("Organisers");

        modelBuilder.Entity<Participant>()
            .ToTable("Participants");

        modelBuilder.Entity<Category>()
            .ToTable("Categories");

        modelBuilder.Entity<Route>()
            .ToTable("Route");

        modelBuilder.Entity<Event>()
            .ToTable("Events")
            .HasIndex(e => new { e.EventsId, e.OrganisersId });

        modelBuilder.Entity<Result>()
            .ToTable("results")
            .HasIndex(r => new { r.EventsId, r.ParticipantsId })
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganisersId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoriesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Route)
            .WithMany(r => r.Events)
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Results)
            .HasForeignKey(r => r.EventsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Participant)
            .WithMany(p => p.Results)
            .HasForeignKey(r => r.ParticipantsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoriesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
