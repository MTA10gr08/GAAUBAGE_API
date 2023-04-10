namespace API.Entities;
public class GTContextClassificationEntity : BaseGTProcessingEntity<UserContextClassificationEntity, GTContextClassificationEntity, string>
{
    public GTContextClassificationEntity() : base() { }
    public Guid GTBackgroundClassificationId { get; set; }
    public GTBackgroundClassificationEntity GTBackgroundClassification { get; set; } = new GTBackgroundClassificationEntity();
    public ICollection<GTTrashCountEntity> GTTrashCount { get; set; } = new List<GTTrashCountEntity>();
    public ICollection<UserTrashCountEntity> UserTrashCount { get; set; } = new List<UserTrashCountEntity>();
    public GTTrashCountEntity? Consensus { get; set; }
    public bool IsInProgress => GTTrashCount.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserTrashCountEntity userProcessing)
    {
        var groundTruth = GTTrashCount.FirstOrDefault(x => x.Data == userProcessing.Data);

        if (groundTruth == null)
        {
            groundTruth = new GTTrashCountEntity
            {
                GTContextClassification = this,
                GTContextClassificationId = Id,
                Data = userProcessing.Data,
            };
            GTTrashCount.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserTrashCount.Add(userProcessing);

        int total = UserTrashCount.Count;
        double threshold = total * 0.75;
        Consensus = GTTrashCount.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}
public class UserContextClassificationEntity : BaseUserProcessingEntity<UserContextClassificationEntity, GTContextClassificationEntity, string>
{
    public UserContextClassificationEntity() : base() { }
}