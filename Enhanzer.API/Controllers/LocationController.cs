using Enhanzer.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLocations(CancellationToken cancellationToken)
    {
        var locations = await dbContext.Locations
            .AsNoTracking()
            .OrderBy(location => location.LocationName)
            .ToListAsync(cancellationToken);
        return Ok(locations);
    }
}
