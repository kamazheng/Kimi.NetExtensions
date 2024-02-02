public class RecordQuery
{
    public string TableClassFullName { get; set; } = string.Empty;
    public object Id { get; set; } = null!;
    public bool IgnoreAutoInclude { get; set; } = false;
    public string[] Includes { get; set; } = new string[] { };
}