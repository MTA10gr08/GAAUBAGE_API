namespace API.Entities;
public class GTBackgroundClassificationEntity : BaseGTProcessingEntity<UserBackgroundClassificationEntity, GTBackgroundClassificationEntity, string>
{
    public GTBackgroundClassificationEntity() : base() {}
    public Guid ImageId { get; set; }
    public ImageEntity Image { get; set; } = null!;
    public ICollection<GTContextClassificationEntity> GTContextClassifications { get; set; } = new List<GTContextClassificationEntity>();
    public ICollection<UserContextClassificationEntity> UserContextClassifications { get; set; } = new List<UserContextClassificationEntity>();
    public GTContextClassificationEntity? Consensus { get; set; }
    public bool IsInProgress => GTContextClassifications.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserContextClassificationEntity userProcessing)
    {
        var groundTruth = GTContextClassifications.FirstOrDefault(x => x.Data == userProcessing.Data);

        if (groundTruth == null)
        {
            groundTruth = new GTContextClassificationEntity
            {
                GTBackgroundClassification = this,
                GTBackgroundClassificationId = Id,
                Data = userProcessing.Data,
            };
            GTContextClassifications.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserContextClassifications.Add(userProcessing);

        int total = UserContextClassifications.Count;
        double threshold = total * 0.75;
        Consensus = GTContextClassifications.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}
public class UserBackgroundClassificationEntity : BaseUserProcessingEntity<UserBackgroundClassificationEntity, GTBackgroundClassificationEntity, string>
{
    public UserBackgroundClassificationEntity() : base() {}
}