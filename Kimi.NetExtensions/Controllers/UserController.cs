using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Model.Identities;
using Kimi.NetExtensions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Kimi.NetExtensions.Controllers;

/// <summary>
/// User controller to get user
/// </summary>
[Route("[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IUser _user;

    /// <summary>
    /// User controller to get user
    /// </summary>
    /// <param name="httpContextAccessor">
    /// </param>
    /// <param name="configuration">
    /// </param>
    public UserController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IUser user)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _user = user;
    }

    /// <summary>
    /// Get user by basic authentication, {"item1":"username","item2":"password"} Only login jwt,
    /// without role and permission returned
    /// </summary>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [AllowAnonymous]
    [Route("Login")]
    public async Task<IActionResult> LogIn([FromBody] (string username, string password) user)
    {
        var md5Pass = SHA.SHAmd5Encrypt(user.password);
        var tableQuery = new TableQuery
        {
            WhereClause = $"{nameof(Model.Identities.User.UserId)}=\"{user.username}\" " +
                            $"&& {nameof(Model.Identities.User.Password)}=\"{md5Pass}\"",
            TableClassFullName = typeof(Model.Identities.User).FullName!,
        };
        var result = await Task.Run(() => DbContextExtension.GetDbRecordsByDynamicLinq(tableQuery, new SysUser()));
        var getUser = result?.ToDynamicList<User>()?.FirstOrDefault();

        if (getUser != null)
        {
            var userName = getUser?.UserId;
            var token = CreateJWT(userName!);
            return Ok(token);
        }
        else
        {
            return BadRequest("Bad username or password!");
        }
    }

    public static string CreateJWT(string userName)
    {
        var secretkey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ConfigReader.Configuration["JWT:Key"]!));
        var credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, userName),
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            };

        var TimeValid = ConfigReader.Configuration["JWT:TimeValid"];
        if (TimeValid == null) throw new Exception("TimeValid Not Set");
        var exprired = int.Parse(TimeValid.ToString());
        var token = new JwtSecurityToken(
            issuer: ConfigReader.Configuration["JWT:Issuer"],
            audience: ConfigReader.Configuration["JWT:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(exprired),
            signingCredentials: credentials
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}