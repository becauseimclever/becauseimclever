namespace BecauseImClever.Server.Controllers;

using System.Reflection;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller that exposes the running application version.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    /// <summary>
    /// Gets the current application version derived from the assembly informational version.
    /// </summary>
    /// <returns>The version string.</returns>
    [HttpGet]
    public IActionResult GetVersion()
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";

        return this.Ok(new VersionResponse(version));
    }
}
