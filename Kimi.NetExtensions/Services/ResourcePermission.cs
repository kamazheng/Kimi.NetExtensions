using Kimi.NetExtensions.Localization;
using Kimi.NetExtensions.Model.Identities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Kimi.NetExtensions.Services;

public static class ResourcePermission
{
    public static string NameFor(string resourceCategory, string resource, string action) => $"{resourceCategory}.{resource}.{action}";

    public static string NameForTableFullName(string tableClassFullName, string action) => NameForTable(tableClassFullName.GetClassType()!, action);

    public static string NameForTable(Type tableClassType, string action) => NameFor(ResourceCategory.Database, tableClassType.Name, action);

    //Todo: Update this to developer self.
    public static List<string> RootUsers { get; set; } = new List<string>() { "kzheng" };

    public const string PolicyTypeName = "permission";

    //Todo: manual add permission
    public static Permission[] Permissions { get; set; } =
    {
        new Permission { Name = NameFor(ResourceCategory.System,"BackgroundTask",ResourceAction.Write),Description = "Background tasks management"},
        new Permission { Name = "AI_Data_Query",Description = "Use natural language to query data by AI"},
    };

    /// <summary>
    /// Get All Resource Permissions for table which implements IReadAccessEntity and
    /// IWriteAccessEntity, and the User define permissions.
    /// </summary>
    /// <returns>
    /// </returns>
    public static List<Permission> GetAllResourcePermissions()
    {
        var list = new List<Permission>();
        var readTables = TypeExtensions.GetTypesForImplementInterface<IReadAccessEntity>();
        foreach (var table in readTables)
        {
            list.Add(new Permission
            {
                Name = NameForTable(table, ResourceAction.Read),
                Description = $"Read access for database table {table.Name}"
            });
        }
        var writeTables = TypeExtensions.GetTypesForImplementInterface<IWriteAccessEntity>();
        foreach (var table in writeTables)
        {
            list.Add(new Permission
            {
                Name = NameForTable(table, ResourceAction.Write),
                Description = $"Write access for database table {table.Name}"
            });
        }

        list.AddRange(Permissions);
        return list;
    }

    /// <summary>
    /// Check user claim, otherwise throw exception.
    /// </summary>
    /// <param name="User">
    /// </param>
    /// <param name="claim">
    /// </param>
    /// <exception cref="Exception">
    /// </exception>
    public static void Authorizing(this ClaimsPrincipal? User, string claim)
    {
        if (User == null || string.IsNullOrEmpty(User?.Identity?.Name)) return; //This is for background job, auto authorized.
        var errorMsg = $"{User?.Identity?.Name} {L.NotAuthorized} {claim}";
        if (claim == null) throw new Exception(errorMsg);
        if (RootUsers?.Any(u => u == User?.Identity?.Name) == true) return;
        if (!User!.HasClaim(c => c.Value == claim && c.Type == PolicyTypeName)) throw new Exception(errorMsg);
    }

    public static bool HasClaim(this ClaimsPrincipal? User, string claim)
    {
        if (User == null) return true; //This is for background job, auto authorized.
        if (claim == null) return false;
        if (RootUsers?.Any(u => u == User?.Identity?.Name) == true) return true;
        if (User.HasClaim(c => c.Value == claim && c.Type == PolicyTypeName)) { return true; } else { return false; };
    }

    public static void RegisterPermissionClaims(AuthorizationOptions options)
    {
        var permissions = GetAllResourcePermissions();
        foreach (var permission in permissions)
        {
            options.AddPolicy(permission.Name, policy => policy.RequireClaim(PolicyTypeName, permission.Name));
        }
    }
}