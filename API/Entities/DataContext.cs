using Microsoft.EntityFrameworkCore;
namespace API.Entities;
public class DataContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ImageEntity> Images => Set<ImageEntity>();
    public DbSet<GTBackgroundClassificationEntity> GTBackgroundClassifications => Set<GTBackgroundClassificationEntity>();
    public DbSet<UserBackgroundClassificationEntity> UserBackgroundClassifications => Set<UserBackgroundClassificationEntity>();
    public DbSet<GTContextClassificationEntity> GTContextClassifications => Set<GTContextClassificationEntity>();
    public DbSet<UserContextClassificationEntity> UserContextClassifications => Set<UserContextClassificationEntity>();
    public DbSet<GTTrashCountEntity> GTTrashCounts => Set<GTTrashCountEntity>();
    public DbSet<UserTrashCountEntity> UserTrashCounts => Set<UserTrashCountEntity>();
    public DbSet<GTTrashBoundingBoxEntity> GTTrashBoundingBoxes => Set<GTTrashBoundingBoxEntity>();
    public DbSet<UserTrashBoundingBoxEntity> UserTrashBoundingBoxes => Set<UserTrashBoundingBoxEntity>();
    public DbSet<GTTrashSuperCategoryEntity> GTTrashSuperCategories => Set<GTTrashSuperCategoryEntity>();
    public DbSet<UserTrashSuperCategoryEntity> UserTrashSuperCategories => Set<UserTrashSuperCategoryEntity>();
    public DbSet<GTTrashCategoryEntity> GTTrashCategories => Set<GTTrashCategoryEntity>();
    public DbSet<UserTrashCategoryEntity> UserTrashCategories => Set<UserTrashCategoryEntity>();
    public DbSet<GTSegmentationEntity> GTSegmentation => Set<GTSegmentationEntity>();
    public DbSet<UserSegmentationEntity> UserSegmentation => Set<UserSegmentationEntity>();
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity<UserEntity>(modelBuilder);
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.Alias).HasMaxLength(100);
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.HasMany(e => e.UserContextCategories).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserBackgroundContexts).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserTrashCounts).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserTrashBoundingBoxes).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserTrashSuperCategories).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserTrashCategories).WithOne(e => e.User).HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserSegmentations).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        ConfigureBaseEntity<ImageEntity>(modelBuilder);
        modelBuilder.Entity<ImageEntity>(entity =>
        {
            entity.Property(e => e.URI).HasMaxLength(2048).IsRequired();
            entity.HasMany(e => e.GTBackgroundClassifications).WithOne(e => e.Image).HasForeignKey(e => e.ImageId);
            entity.HasMany(e => e.UserBackgroundClassifications);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<GTBackgroundClassificationEntity>(modelBuilder);
        modelBuilder.Entity<GTBackgroundClassificationEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.Image).WithMany(e => e.GTBackgroundClassifications).HasForeignKey(e => e.ImageId).IsRequired();
            entity.HasMany(e => e.GTContextClassifications).WithOne(e => e.GTBackgroundClassification).HasForeignKey(e => e.GTBackgroundClassificationId);
            entity.HasMany(e => e.UserContextClassifications);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserBackgroundClassificationEntity>(modelBuilder);
        modelBuilder.Entity<UserBackgroundClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserBackgroundContexts).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });

        ConfigureBaseEntity<GTContextClassificationEntity>(modelBuilder);
        modelBuilder.Entity<GTContextClassificationEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.GTBackgroundClassification).WithMany(e => e.GTContextClassifications).HasForeignKey(e => e.GTBackgroundClassificationId).IsRequired();
            entity.HasMany(e => e.GTTrashCount).WithOne(e => e.GTContextClassification).HasForeignKey(e => e.GTContextClassificationId);
            entity.HasMany(e => e.UserTrashCount);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserContextClassificationEntity>(modelBuilder);
        modelBuilder.Entity<UserContextClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserContextCategories).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });

        ConfigureBaseEntity<GTTrashCountEntity>(modelBuilder);
        modelBuilder.Entity<GTTrashCountEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.GTContextClassification).WithMany(e => e.GTTrashCount).HasForeignKey(e => e.GTContextClassificationId).IsRequired();
            entity.HasMany(e => e.GTTrashBoundingBoxes).WithOne(e => e.GTTrashCount).HasForeignKey(e => e.GTTrashCountId);
            entity.HasMany(e => e.UserTrashBoundingBoxes);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserTrashCountEntity>(modelBuilder);
        modelBuilder.Entity<UserTrashCountEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserTrashCounts).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });

        modelBuilder.Entity<RectangleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.GTTrashBoundingBoxEntity)
                .WithMany(e => e.Data)
                .HasForeignKey(e => e.GTTrashBoundingBoxEntityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // This line is important to make the foreign key nullable

            entity.HasOne(e => e.UserTrashBoundingBoxEntity)
                .WithMany(e => e.Data)
                .HasForeignKey(e => e.UserTrashBoundingBoxEntityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // This line is important to make the foreign key nullable
        });

        ConfigureBaseEntity<GTTrashBoundingBoxEntity>(modelBuilder);
        modelBuilder.Entity<GTTrashBoundingBoxEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.HasMany(e => e.Data).WithOne(e => e.GTTrashBoundingBoxEntity).HasForeignKey(e => e.GTTrashBoundingBoxEntityId).OnDelete(DeleteBehavior.Cascade);
            /*entity.OwnsMany(e => e.Data, data =>
                {
                    data.WithOwner();
                    data.Property(p => p.X).HasColumnName("X");
                    data.Property(p => p.Y).HasColumnName("Y");
                    data.Property(p => p.Width).HasColumnName("Width");
                    data.Property(p => p.Height).HasColumnName("Height");
                });*/
            entity.HasOne(e => e.GTTrashCount).WithMany(e => e.GTTrashBoundingBoxes).HasForeignKey(e => e.GTTrashCountId).IsRequired();
            entity.HasMany(e => e.GTTrashSuperCategories).WithOne(e => e.GTTrashBoundingBox).HasForeignKey(e => e.GTTrashBoundingBoxId);
            entity.HasMany(e => e.UserTrashSuperCategories);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserTrashBoundingBoxEntity>(modelBuilder);
        modelBuilder.Entity<UserTrashBoundingBoxEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserTrashBoundingBoxes).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.HasMany(e => e.Data).WithOne(e => e.UserTrashBoundingBoxEntity).HasForeignKey(e => e.UserTrashBoundingBoxEntityId).OnDelete(DeleteBehavior.Cascade);
            /*entity.OwnsMany(e => e.Data, data =>
                {
                    data.WithOwner();
                    data.Property(p => p.X).HasColumnName("X");
                    data.Property(p => p.Y).HasColumnName("Y");
                    data.Property(p => p.Width).HasColumnName("Width");
                    data.Property(p => p.Height).HasColumnName("Height");
                });*/
        });

        ConfigureBaseEntity<GTTrashSuperCategoryEntity>(modelBuilder);
        modelBuilder.Entity<GTTrashSuperCategoryEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.GTTrashBoundingBox).WithMany(e => e.GTTrashSuperCategories).HasForeignKey(e => e.GTTrashBoundingBoxId).IsRequired();
            entity.HasMany(e => e.GTTrashCategory).WithOne(e => e.GTTrashSuperCategory).HasForeignKey(e => e.GTTrashSuperCategoryId);
            entity.HasMany(e => e.UserTrashCategory);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserTrashSuperCategoryEntity>(modelBuilder);
        modelBuilder.Entity<UserTrashSuperCategoryEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserTrashSuperCategories).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });

        ConfigureBaseEntity<GTTrashCategoryEntity>(modelBuilder);
        modelBuilder.Entity<GTTrashCategoryEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.GTTrashSuperCategory).WithMany(e => e.GTTrashCategory).HasForeignKey(e => e.GTTrashSuperCategoryId).IsRequired();
            entity.HasMany(e => e.GTSegmentations).WithOne(e => e.GTTrashCategory).HasForeignKey(e => e.GTTrashCategoryId);
            entity.HasMany(e => e.UserSegmentations);
            entity.HasOne(e => e.Consensus);
        });

        ConfigureBaseEntity<UserTrashCategoryEntity>(modelBuilder);
        modelBuilder.Entity<UserTrashCategoryEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserTrashCategories).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });

        ConfigureBaseEntity<GTSegmentationEntity>(modelBuilder);
        modelBuilder.Entity<GTSegmentationEntity>(entity =>
        {
            entity.HasMany(e => e.UserProcessings).WithOne(e => e.GTProcessing).HasForeignKey(e => e.GTProcessingId);
            entity.Property(e => e.Data).IsRequired();
            entity.HasOne(e => e.GTTrashCategory).WithMany(e => e.GTSegmentations).HasForeignKey(e => e.GTTrashCategoryId).IsRequired();
        });

        ConfigureBaseEntity<UserSegmentationEntity>(modelBuilder);
        modelBuilder.Entity<UserSegmentationEntity>(entity =>
        {
            entity.HasOne(e => e.User).WithMany(e => e.UserSegmentations).HasForeignKey(e => e.UserId).IsRequired();
            entity.HasOne(e => e.GTProcessing).WithMany(e => e.UserProcessings).HasForeignKey(e => e.GTProcessingId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateBaseEntityDates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateBaseEntityDates();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateBaseEntityDates()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((BaseEntity)entry.Entity).Created = DateTime.UtcNow;
            }

            ((BaseEntity)entry.Entity).Updated = DateTime.UtcNow;
        }
    }

    private static void ConfigureBaseEntity<TEntity>(ModelBuilder modelBuilder)
    where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Updated).IsRequired();
            entity.Property(e => e.Created).IsRequired();
        });
    }
}