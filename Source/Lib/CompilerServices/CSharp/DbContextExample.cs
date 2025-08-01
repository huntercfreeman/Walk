using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Walk.CompilerServices.CSharp;

public class TestDbContext : DbContext
{
    public DbSet<Name> Names { get; set; } = default!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Name>().ToTable("Names");
        modelBuilder.Entity<Name>().HasIndex(x => x.FullName);
        modelBuilder.Entity<Name>().Property(x => x.FullName).UseCollation("nocase");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.LogTo(Console.WriteLine, LogLevel.Warning)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging(true);
    }
}
