using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;

namespace API.Entities;
public class DataContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ImageEntity> Images => Set<ImageEntity>();
    public DbSet<ImageAnnotationEntity> ImageAnnotations => Set<ImageAnnotationEntity>();
    public DbSet<BackgroundClassificationEntity> BackgroundClassifications => Set<BackgroundClassificationEntity>();
    public DbSet<BackgroundClassificationStringEntity> BackgroundClassificationStrings => Set<BackgroundClassificationStringEntity>();
    public DbSet<ContextClassificationEntity> ContextClassifications => Set<ContextClassificationEntity>();
    public DbSet<SubImageAnnotationGroupEntity> SubImageGroups => Set<SubImageAnnotationGroupEntity>();
    public DbSet<SubImageAnnotationEntity> SubImageAnnotations => Set<SubImageAnnotationEntity>();
    public DbSet<TrashSuperCategoryEntity> TrashSuperCategories => Set<TrashSuperCategoryEntity>();
    public DbSet<TrashSubCategoryEntity> TrashSubCategories => Set<TrashSubCategoryEntity>();
    public DbSet<SegmentationEntity> Segmentations => Set<SegmentationEntity>();

    public DataContext(DbContextOptions<DataContext> options) : base(options)
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
            entity.HasMany(e => e.BackgroundClassifications).WithMany(e => e.Users);
            entity.HasMany(e => e.ContextClassifications).WithMany(e => e.Users);
            entity.HasMany(e => e.SubImageAnnotationGroups).WithMany(e => e.Users);
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
            entity.HasOne(e => e.Image).WithOne(e => e.ImageAnnotation).HasForeignKey<ImageEntity>(e => e.ImageAnnotationID);
            entity.HasMany(e => e.BackgroundClassifications).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.ContextClassifications).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.SubImageAnnotationGroups).WithOne(e => e.ImageAnnotation).HasForeignKey(e => e.ImageAnnotationID);
        });

        ConfigureBaseEntity<BackgroundClassificationEntity>(modelBuilder);
        modelBuilder.Entity<BackgroundClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.BackgroundClassifications).HasForeignKey(e => e.ImageAnnotationID);
            entity.HasMany(e => e.BackgroundClassificationStrings).WithOne(e => e.BackgroundClassification).HasForeignKey(e => e.BackgroundClassificationID);
            entity.HasMany(e => e.Users).WithMany(e => e.BackgroundClassifications);
        });

        ConfigureBaseEntity<BackgroundClassificationStringEntity>(modelBuilder);
        modelBuilder.Entity<BackgroundClassificationStringEntity>(entity =>
        {
            entity.HasOne(e => e.BackgroundClassification).WithMany(e => e.BackgroundClassificationStrings).HasForeignKey(e => e.BackgroundClassificationID);
            entity.Property(e => e.value).HasMaxLength(1000);
        });

        ConfigureBaseEntity<ContextClassificationEntity>(modelBuilder);
        modelBuilder.Entity<ContextClassificationEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.ContextClassifications).HasForeignKey(e => e.ImageAnnotationID);
            entity.Property(e => e.ContextClassification).HasMaxLength(1000);
            entity.HasMany(e => e.Users).WithMany(e => e.ContextClassifications);
        });

        ConfigureBaseEntity<SubImageAnnotationGroupEntity>(modelBuilder);
        modelBuilder.Entity<SubImageAnnotationGroupEntity>(entity =>
        {
            entity.HasOne(e => e.ImageAnnotation).WithMany(e => e.SubImageAnnotationGroups).HasForeignKey(e => e.ImageAnnotationID).IsRequired();
            entity.HasMany(e => e.Users).WithMany(e => e.SubImageAnnotationGroups);
            entity.HasMany(e => e.SubImageAnnotations).WithOne(e => e.SubImageAnnotationGroup).HasForeignKey(e => e.SubImageAnnotationGroupID).IsRequired();
        });

        ConfigureBaseEntity<SubImageAnnotationEntity>(modelBuilder);
        modelBuilder.Entity<SubImageAnnotationEntity>(entity =>
        {
            entity.HasOne(e => e.SubImageAnnotationGroup).WithMany(e => e.SubImageAnnotations).HasForeignKey(e => e.SubImageAnnotationGroupID).IsRequired();
            entity.HasMany(e => e.TrashSubCategories).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.HasMany(e => e.TrashSuperCategories).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.HasMany(e => e.Segmentations).WithOne(e => e.SubImageAnnotation).HasForeignKey(e => e.SubImageAnnotationID).IsRequired();
            entity.Property(e => e.X);
            entity.Property(e => e.Y);
            entity.Property(e => e.Width);
            entity.Property(e => e.Height);
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