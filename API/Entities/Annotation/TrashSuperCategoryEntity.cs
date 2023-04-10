namespace API.Entities;
public class GTTrashSuperCategoryEntity : BaseGTProcessingEntity<UserTrashSuperCategoryEntity, GTTrashSuperCategoryEntity, string>
{
    public GTTrashSuperCategoryEntity() : base() {}
    public Guid GTTrashBoundingBoxId { get; set; }
    public GTTrashBoundingBoxEntity GTTrashBoundingBox { get; set; } = null!;
    public ICollection<GTTrashCategoryEntity> GTTrashCategory { get; set; } = new List<GTTrashCategoryEntity>();
    public ICollection<UserTrashCategoryEntity> UserTrashCategory { get; set; } = new List<UserTrashCategoryEntity>();
    public GTTrashCategoryEntity? Consensus { get; set; }
    public bool IsInProgress => GTTrashCategory.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserTrashCategoryEntity userProcessing)
    {
        var groundTruth = GTTrashCategory.FirstOrDefault(x => x.Data == userProcessing.Data);

        if (groundTruth == null)
        {
            groundTruth = new GTTrashCategoryEntity
            {
                GTTrashSuperCategory = this,
                GTTrashSuperCategoryId = Id,
                Data = userProcessing.Data,
            };
            GTTrashCategory.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserTrashCategory.Add(userProcessing);

        int total = UserTrashCategory.Count;
        double threshold = total * 0.75;
        Consensus = GTTrashCategory.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}
public class UserTrashSuperCategoryEntity : BaseUserProcessingEntity<UserTrashSuperCategoryEntity, GTTrashSuperCategoryEntity, string>
{
    public UserTrashSuperCategoryEntity() : base() {}
}