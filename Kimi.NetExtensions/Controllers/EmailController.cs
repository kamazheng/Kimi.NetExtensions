using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Kimi.NetExtensions.Controllers;

/// <summary>
/// Email webapi
/// </summary>
[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    /// <summary>
    /// Send emmail
    /// </summary>
    /// <param name="email">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [Route("SendEmail")]
    public IActionResult SendEmail(Email email)
    {
        BackgroundJob.Enqueue(() => EmailService.Send(email, User.Identity!.Name!));
        return Ok();
    }
}