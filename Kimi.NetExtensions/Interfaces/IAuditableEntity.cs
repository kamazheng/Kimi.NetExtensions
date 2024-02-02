//Don't implement other entity interface !!!
public interface IAuditableEntity
{
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
}