using DataProcessingApp.Core.DataObjects;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DataProcessingApp.DataAccess;

/// <summary>
/// EF Core model over the actuarial factor tables. Entities are the shared
/// row types from Core; the schema is owned by the EF migrations in this
/// project. The tables have no primary keys, so every entity is keyless and
/// reads are always no-tracking.
/// </summary>
public class ActuarialDbContext : DbContext
{
    private readonly string _connectionString;

    public ActuarialDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // series-backed tables: series is the census year in MortalityTable.
        // The published grids are complete, so all columns are NOT NULL.
        modelBuilder.Entity<TableSRow>(e =>
        {
            e.ToTable("tblS");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableU1Row>(e =>
        {
            e.ToTable("tblU1");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableU2Row>(e =>
        {
            e.ToTable("tblU2");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableR2Row>(e =>
        {
            e.ToTable("tblR2");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableCRow>(e =>
        {
            e.ToTable("tblC");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableHRow>(e =>
        {
            e.ToTable("tblH");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableZRow>(e =>
        {
            e.ToTable("tblZ");
            e.HasNoKey();
        });

        modelBuilder.Entity<MortalityTableRow>(e =>
        {
            e.ToTable("tblMortality");
            e.HasNoKey();
        });

        // rate-only tables: shared across series
        modelBuilder.Entity<TableBRow>(e =>
        {
            e.ToTable("tblB");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableDRow>(e =>
        {
            e.ToTable("tblD");
            e.HasNoKey();
        });

        modelBuilder.Entity<TableFRow>(e =>
        {
            e.ToTable("tblF");
            e.HasNoKey();
            e.Property(r => r.Frequency).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<TableJRow>(e =>
        {
            e.ToTable("tblJ");
            e.HasNoKey();
            e.Property(r => r.Frequency).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<TableKRow>(e =>
        {
            e.ToTable("tblK");
            e.HasNoKey();
            e.Property(r => r.Frequency).HasMaxLength(255).IsRequired();
        });

        ApplyDbColumnNames(modelBuilder);
    }

    /// <summary>
    /// The SQL column names come from the [DbColumn] attributes on the row
    /// classes (mixed case like pvAnnuity, dFactor); the bulk-copy seeder
    /// maps columns by exact name, so EF must use them verbatim instead of
    /// the CLR property names.
    /// </summary>
    private static void ApplyDbColumnNames(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var attribute = property.PropertyInfo?.GetCustomAttribute<DbColumnAttribute>();
                if (attribute != null)
                {
                    property.SetColumnName(attribute.Name);
                }
            }
        }
    }
}