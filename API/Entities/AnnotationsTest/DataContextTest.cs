using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;

namespace API.EntitiesTest;
public class DataContextTest : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ImageEntity> Images => Set<ImageEntity>();
    public DbSet<ImageAnnotationEntity> ImageAnnotations => Set<ImageAnnotationEntity>();
    public DbSet<BackgroundClassificationEntity> BackgroundClassifications => Set<BackgroundClassificationEntity>();
    public DbSet<BackgroundClassificationLabelEntity> BackgroundClassificationLabels => Set<BackgroundClassificationLabelEntity>();
    public DbSet<ContextClassificationEntity> ContextClassifications => Set<ContextClassificationEntity>();
    public DbSet<BoundingBoxEntity> BoundingBoxes => Set<BoundingBoxEntity>();
    public DbSet<SubImageGroupEntity> SubImageGroups => Set<SubImageGroupEntity>();
    public DbSet<SubImageAnnotationEntity> SubImageAnnotations => Set<SubImageAnnotationEntity>();
    public DbSet<TrashSuperCategoryEntity> TrashSuperCategories => Set<TrashSuperCategoryEntity>();
    public DbSet<TrashSubCategoryEntity> TrashSubCategories => Set<TrashSubCategoryEntity>();
    public DbSet<SegmentationEntity> Segmentations => Set<SegmentationEntity>();

    public DataContextTest(DbContextOptions<DataContextTest> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity<UserEntity>(modelBuilder);
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.Alias).HasMaxLength(100);
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.HasMany(e => e.Images).WithOne(e => e.User).HasForeignKey(e => e.UserID);
            entity.HasMany(e => e.BackgroundClassificationLabels).WithMany(e => e.Users);
            entity.HasMany(e => e.ContextClassifications).WithMany(e => e.Users);
            entity.HasMany(e => e.SubImageGroups).WithOne(e => e.User).HasForeignKey(e => e.UserID);
            entity.HasMany(e => e.TrashSuperCategories).WithMany(e => e.Users);
            entity.HasMany(e => e.TrashSubCategories).WithMany(e => e.Users);
            entity.HasMany(e => e.Segmentations).WithOne(e => e.User).HasForeignKey(e => e.UserID);
        });

        ConfigureBaseEntity<ImageEntity>(modelBuilder);
        modelBuilder.Entity<ImageEntity>(entity =>
        {
            entity.Property(e => e.URI).HasMaxLength(1000);
            entity.HasOne(e => e.User).WithMany(e => e.Images).HasForeignKey(e => e.UserID);
            entity.HasOne(e => e.ImageAnnotation).WithOne(e => e.Image).HasForeignKey<ImageAnnotationEntity>(e => e.ImageID);
        });

        modelBuilder.Entity<ImageAnnotationEntity>(entity =>
        {
            entity.HasOne(e => e.Image).WithOne(e => e.ImageAnnotation).HasForeignKey<ImageEntity>(e => e.ImageAnnotationId);
            entity.HasOne(e => e.BackgroundClassificationConsensus).WithMany().HasForeignKey(e => e.BackgroundClassificationConsensusId);
            entity.HasMany(e => e.BackgroundClassifications).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasOne(e => e.ContextClassificationConsensus).WithMany().HasForeignKey(e => e.ContextClassificationConsensusId);
            entity.HasMany(e => e.ContextClassifications).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.SubImagesConsensus).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.SubImages).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
        });

        ConfigureBaseEntity<BackgroundClassificationEntity>(modelBuilder);
        modelBuilder.Entity<BackgroundClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.BackgroundClassifications).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.BackgroundClassificationLabels).WithOne(e => e.BackgroundClassification).HasForeignKey(e => e.BackgroundClassificationID);
        });

        ConfigureBaseEntity<BackgroundClassificationLabelEntity>(modelBuilder);
        modelBuilder.Entity<BackgroundClassificationLabelEntity>(entity =>
        {
            entity.HasOne(e => e.BackgroundClassification).WithMany(e => e.BackgroundClassificationLabels).HasForeignKey(e => e.BackgroundClassificationID);
            entity.Property(e => e.BackgroundClassificationLabel).HasMaxLength(1000);
            entity.HasMany(e => e.Users).WithMany(e => e.BackgroundClassificationLabels);
        });

        ConfigureBaseEntity<ContextClassificationEntity>(modelBuilder);
        modelBuilder.Entity<ContextClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.ContextClassifications).HasForeignKey(e => e.ImageAnnotationID);
            entity.Property(e => e.ContextClassification).HasMaxLength(1000);
            entity.HasMany(e => e.Users).WithMany(e => e.ContextClassifications);
        });

        ConfigureBaseEntity<BoundingBoxEntity>(modelBuilder);
        modelBuilder.Entity<BoundingBoxEntity>(entity =>
        {
            entity.HasOne(e => e.SubImageAnnotation).WithOne(e => e.SubImage).HasForeignKey<SubImageAnnotationEntity>(e => e.SubImageID).IsRequired(false);
            entity.HasOne(e => e.SubImageGroup).WithMany(e => e.SubImages).HasForeignKey(e => e.SubImageGroupID).IsRequired(false);
            entity.Property(e => e.X).IsRequired();
            entity.Property(e => e.Y).IsRequired();
            entity.Property(e => e.Width).IsRequired();
            entity.Property(e => e.Height).IsRequired();
        });

        ConfigureBaseEntity<SubImageGroupEntity>(modelBuilder);
        modelBuilder.Entity<SubImageGroupEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.SubImages).HasForeignKey(e => e.ImageAnnotationID).IsRequired();
            entity.HasOne(e => e.User).WithMany(e => e.SubImageGroups).HasForeignKey(e => e.UserID).IsRequired();
            entity.HasMany(e => e.SubImages).WithOne(e => e.SubImageGroup).HasForeignKey(e => e.SubImageGroupID);
        });

        ConfigureBaseEntity<SubImageAnnotationEntity>(modelBuilder);
        modelBuilder.Entity<SubImageAnnotationEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.SubImagesConsensus).HasForeignKey(e => e.ImageAnnotationID).IsRequired();
            entity.HasOne(e => e.SubImage).WithOne(e => e.SubImageAnnotation).HasForeignKey<BoundingBoxEntity>(e => e.SubImageAnnotationID).IsRequired();
            entity.HasOne(e => e.TrashSubCategoryConsensus).WithOne(e => e.SubImageAnnotation).HasForeignKey<TrashSubCategoryEntity>(e => e.SubImageAnnotationID).IsRequired(false);
            entity.HasMany(e => e.TrashSubCategories).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired(false);
            entity.HasOne(e => e.TrashSuperCategoryConsensus).WithOne(e => e.SubImageAnnotation).HasForeignKey<TrashSuperCategoryEntity>(e => e.SubImageAnnotationID).IsRequired(false);
            entity.HasMany(e => e.TrashSuperCategories).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired(false);
            entity.HasOne(e => e.SegmentationConsensus).WithOne(e => e.SubImageAnnotation).HasForeignKey<SegmentationEntity>(e => e.SubImageAnnotationID).IsRequired(false);
            entity.HasMany(e => e.Segmentations).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired(false);
        });

        ConfigureBaseEntity<TrashSubCategoryEntity>(modelBuilder);
        modelBuilder.Entity<TrashSubCategoryEntity>(entity =>
        {
            entity.HasOne(e => e.SubImageAnnotation).WithMany(e => e.TrashSubCategories).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.HasMany(e => e.Users).WithMany(e => e.TrashSubCategories);
            entity.Property(e => e.TrashSubCategory).HasMaxLength(1000);
        });

        ConfigureBaseEntity<TrashSuperCategoryEntity>(modelBuilder);
        modelBuilder.Entity<TrashSuperCategoryEntity>(entity =>
        {
            entity.HasOne(e => e.SubImageAnnotation).WithMany(e => e.TrashSuperCategories).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.HasMany(e => e.Users).WithMany(e => e.TrashSuperCategories);
            entity.Property(e => e.TrashSuperCategory).HasMaxLength(1000);
        });

        ConfigureBaseEntity<SegmentationEntity>(modelBuilder);
        modelBuilder.Entity<SegmentationEntity>(entity =>
        {
            entity.HasOne(e => e.SubImageAnnotation).WithMany(e => e.Segmentations).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.HasOne(e => e.User).WithMany(e => e.Segmentations).HasForeignKey(e => e.UserID).IsRequired();
            entity.Property(e => e.Segmentation).IsRequired();
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
            entity.HasKey(e => e.ID);
            entity.Property(e => e.Updated).IsRequired();
            entity.Property(e => e.Created).IsRequired();
        });
    }
}