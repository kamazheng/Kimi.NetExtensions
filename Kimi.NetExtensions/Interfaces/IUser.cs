namespace Kimi.NetExtensions.Interfaces;

public interface IUser
{
    string UserName { get; }
    string? Domain { get; set; }
    string? Email { get; set; }
    string? EmulateUserName { get; set; }
    string? FullName { get; set; }
    bool IsEmulate { get { return !string.IsNullOrEmpty(EmulateUserName); } }
    string? JWT { get; set; }
    List<string>? Permissions { get; }
    List<string>? Roles { get; }
    bool IsRootUser { get; }

    bool CanReadTable(string tableFullName);

    bool CanWriteTable(string tableFullName);
    void ResetUser();
}

public class SysUser : IUser
{
    public string UserName { get; }

    public string? Domain { get; set; }
    public string? Email { get; set; }
    public string? EmulateUserName { get; set; }
    public string? FullName { get; set; }
    public string? JWT { get; set; }

    public List<string>? Permissions { get; }

    public List<string>? Roles { get; }

    public bool IsRootUser => false;

    public bool CanReadTable(string tableName)
    {
        return true;
    }

    public bool CanWriteTable(string tableName)
    {
        return true;
    }

    public void ResetUser()
    {
        throw new NotImplementedException();
    }

    public SysUser()
    {
        UserName = "System";
    }
}