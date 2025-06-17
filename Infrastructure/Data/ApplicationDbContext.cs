using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }    
    public DbSet<Player> Players { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<PlayerSkill> PlayerSkills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.PlayerSkills)
            .WithOne(ps => ps.Player)
            .HasForeignKey(ps => ps.PlayerId);modelBuilder.Entity<Team>()
            .HasMany(t => t.Players)
            .WithMany(p => p.Teams)
            .UsingEntity<Dictionary<string, object>>(
                "TeamPlayers",
                j => j.HasOne<Player>().WithMany(),
                j => j.HasOne<Team>().WithMany());
    }
}
