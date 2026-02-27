using Application.Features.Cors;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

[Authorize(Roles = "Admin")]                           // restrict to administrators or adjust policy as needed
public class CorsOriginsController(SharedDbContext context, ICorsOriginService corsService) : BaseApiController
{
    private readonly SharedDbContext _context = context;
    private readonly ICorsOriginService _corsService = corsService;

    /// <summary>
    /// Adds a new allowed origin.  Only authenticated admins may call this endpoint.
    /// </summary>
    /// <param name="origin">Full URI (scheme+host[,port]) to allow.</param>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return BadRequest("Origin is required.");

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            return BadRequest("Invalid origin URL. Must be a valid http or https URI.");
        }

        // prevent duplicates
        if (await _context.CorsOrigins.AnyAsync(x => x.Origin == origin))
        {
            return Conflict("Origin already registered.");
        }

        var entity = new CorsOrigin { Origin = origin, IsActive = true };
        _context.CorsOrigins.Add(entity);
        await _context.SaveChangesAsync();

        _corsService.ClearCache();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // additional convenience endpoints (optional)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.CorsOrigins.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }
}
