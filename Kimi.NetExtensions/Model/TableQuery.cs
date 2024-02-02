public class TableQuery
{
    public string? TableClassFullName { get; set; }
    public string[]? Fields { get; set; }
    public string? WhereClause { get; set; }
    public string? OrderBy { get; set; }
    public int PageSize { get; set; } = 1000;
    public int Page { get; set; } = 1;

    public bool IgnoreAutoInclude { get; set; } = false;
    public string[] Includes { get; set; } = new string[] { };
}