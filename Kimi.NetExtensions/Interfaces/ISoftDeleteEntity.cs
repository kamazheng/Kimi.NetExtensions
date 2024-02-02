public interface ISoftDeleteEntity
{
    string Updatedby { get; set; }
    DateTime Updated { get; set; }
    bool Active { get; set; }
}