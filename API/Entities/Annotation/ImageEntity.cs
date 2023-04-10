namespace API.Entities;
public class ImageEntity : BaseEntity
{
    public ImageEntity() : base() {}
    public string URI { get; set; } = string.Empty;
    public ICollection<GTBackgroundClassificationEntity> GTBackgroundClassifications { get; set; } = new List<GTBackgroundClassificationEntity>();
    public ICollection<UserBackgroundClassificationEntity> UserBackgroundClassifications { get; set; } = new List<UserBackgroundClassificationEntity>();
    public GTBackgroundClassificationEntity? Consensus { get; set; }
    public bool IsInProgress => GTBackgroundClassifications.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserBackgroundClassificationEntity userProcessing)
    {
        var groundTruth = GTBackgroundClassifications.FirstOrDefault(x => x.Data == userProcessing.Data);

        if (groundTruth == null)
        {
            groundTruth = new GTBackgroundClassificationEntity
            {
                Image = this,
                ImageId = Id,
                Data = userProcessing.Data,
            };
            GTBackgroundClassifications.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserBackgroundClassifications.Add(userProcessing);

        int total = UserBackgroundClassifications.Count;
        double threshold = total * 0.75;
        Consensus = GTBackgroundClassifications.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }
}