namespace API.Entities;

public class UserEntity : BaseEntity
{
    public UserEntity() : base() {}
    public string Alias { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;

    public ICollection<UserContextClassificationEntity> UserContextCategories { get; set; } = new List<UserContextClassificationEntity>();
    public ICollection<UserBackgroundClassificationEntity> UserBackgroundContexts { get; set; } = new List<UserBackgroundClassificationEntity>();
    public ICollection<UserTrashCountEntity> UserTrashCounts { get; set; } = new List<UserTrashCountEntity>();
    public ICollection<UserTrashBoundingBoxEntity> UserTrashBoundingBoxes { get; set; } = new List<UserTrashBoundingBoxEntity>();
    public ICollection<UserTrashSuperCategoryEntity> UserTrashSuperCategories { get; set; } = new List<UserTrashSuperCategoryEntity>();
    public ICollection<UserTrashCategoryEntity> UserTrashCategories { get; set; } = new List<UserTrashCategoryEntity>();
    public ICollection<UserSegmentationEntity> UserSegmentations { get; set; } = new List<UserSegmentationEntity>();
}