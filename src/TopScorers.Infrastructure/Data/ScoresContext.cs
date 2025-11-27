using Microsoft.EntityFrameworkCore;
using TopScorers.Core.Models;

namespace TopScorers.Infrastructure.Data;

public class ScoresContext(DbContextOptions<ScoresContext> options) : DbContext(options)
{
    public DbSet<PersonScore> Scores => Set<PersonScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonScore>(entity =>
        {
            entity.ToTable("Scores");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();
            entity.Property(x => x.SecondName)
                  .HasMaxLength(200)
                  .IsRequired();
            entity.Property(x => x.Score)
                  .IsRequired();
            entity.HasIndex(x => x.Score);
        });
    }
}

