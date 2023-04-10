namespace API.Entities;
public class GTTrashCategoryEntity : BaseGTProcessingEntity<UserTrashCategoryEntity, GTTrashCategoryEntity, string>
{
    public GTTrashCategoryEntity() : base() {}
    public Guid GTTrashSuperCategoryId { get; set; }
    public GTTrashSuperCategoryEntity GTTrashSuperCategory { get; set; } = null!;
    public ICollection<GTSegmentationEntity> GTSegmentations { get; set; } = new List<GTSegmentationEntity>();
    public ICollection<UserSegmentationEntity> UserSegmentations { get; set; } = new List<UserSegmentationEntity>();
    public GTSegmentationEntity? Consensus { get; set; }
    public bool IsInProgress => GTSegmentations.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserSegmentationEntity userProcessing)
    {
        var groundTruth = GTSegmentations.FirstOrDefault(x => x.Data == userProcessing.Data);

        if (groundTruth == null)
        {
            groundTruth = new GTSegmentationEntity
            {
                GTTrashCategory = this,
                GTTrashCategoryId = Id,
                Data = userProcessing.Data,
            };
            GTSegmentations.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserSegmentations.Add(userProcessing);

        int total = UserSegmentations.Count;
        double threshold = total * 0.75;
        Consensus = GTSegmentations.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}
public class UserTrashCategoryEntity : BaseUserProcessingEntity<UserTrashCategoryEntity, GTTrashCategoryEntity, string>
{
    public UserTrashCategoryEntity() : base() {}
}