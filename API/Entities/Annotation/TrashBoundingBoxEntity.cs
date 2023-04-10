namespace API.Entities;
public class GTTrashBoundingBoxEntity : BaseGTProcessingEntity<UserTrashBoundingBoxEntity, GTTrashBoundingBoxEntity, List<RectangleEntity>>
{
    public GTTrashBoundingBoxEntity() : base() {}
    public Guid GTTrashCountId { get; set; }
    public GTTrashCountEntity GTTrashCount { get; set; } = null!;
    public ICollection<GTTrashSuperCategoryEntity> GTTrashSuperCategories { get; set; } = new List<GTTrashSuperCategoryEntity>();
    public ICollection<UserTrashSuperCategoryEntity> UserTrashSuperCategories { get; set; } = new List<UserTrashSuperCategoryEntity>();
    public GTTrashSuperCategoryEntity? Consensus { get; set; }
    public bool IsInProgress => GTTrashSuperCategories.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserTrashSuperCategoryEntity userProcessing)
    {
        var groundTruth = GTTrashSuperCategories.FirstOrDefault(x => x.Data == userProcessing.Data); //uhhh nmeds more consideration

        if (groundTruth == null)
        {
            groundTruth = new GTTrashSuperCategoryEntity
            {
                GTTrashBoundingBox = this,
                GTTrashBoundingBoxId = Id,
                Data = userProcessing.Data,
            };
            GTTrashSuperCategories.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserTrashSuperCategories.Add(userProcessing);

        int total = UserTrashSuperCategories.Count;
        double threshold = total * 0.75;
        Consensus = GTTrashSuperCategories.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}
public class UserTrashBoundingBoxEntity : BaseUserProcessingEntity<UserTrashBoundingBoxEntity, GTTrashBoundingBoxEntity, List<RectangleEntity>>
{
    public UserTrashBoundingBoxEntity() : base() {}
}