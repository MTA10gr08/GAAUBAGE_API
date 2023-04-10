namespace API.Entities;
public abstract class BaseGTProcessingEntity<TUser, TGT, D> : BaseEntity
    where TUser : BaseUserProcessingEntity<TUser, TGT, D>
    where TGT : BaseGTProcessingEntity<TUser, TGT, D>
{
    public BaseGTProcessingEntity() : base() {}
    public ICollection<TUser> UserProcessings { get; set; } = new List<TUser>();
    public D Data { get; set; } = default!;
}
public abstract class BaseUserProcessingEntity<TUser, TGT, D> : BaseEntity
    where TUser : BaseUserProcessingEntity<TUser, TGT, D>
    where TGT : BaseGTProcessingEntity<TUser, TGT, D>
{
    public BaseUserProcessingEntity() : base() {}
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public Guid GTProcessingId { get; set; }
    public TGT GTProcessing { get; set; } = null!;
    public D Data { get; set; } = default!;
}