namespace API.Entities;
public abstract class BaseEntity
{
    public BaseEntity() {}
    internal Guid Id { get; set; }
    internal DateTime Created { get; set; } = DateTime.Now;
    internal DateTime Updated { get; set; } = DateTime.Now;
    public static implicit operator bool(BaseEntity? d) => d != null;
}