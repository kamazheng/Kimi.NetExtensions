using System.Security.Claims;


public interface IDbAuthenticate
{
    public bool CanRead(ClaimsPrincipal? User, string tableClassFullName);
    public bool CanWrite(ClaimsPrincipal? User, string tableClassFullName);
}