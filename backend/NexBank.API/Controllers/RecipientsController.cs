using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Infrastructure.Persistence;
using System.Security.Claims;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipientsController : ControllerBase
{
    private readonly NexBankDbContext _context;

    public RecipientsController(NexBankDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavoriteRecipients()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var recipients = await _context.FavoriteRecipients
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Ok(recipients);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavoriteRecipient([FromBody] FavoriteRecipient recipient)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        recipient.UserId = userId;
        recipient.CreatedAt = DateTime.UtcNow;

        _context.FavoriteRecipients.Add(recipient);
        await _context.SaveChangesAsync();
        return Ok(recipient);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipient(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var recipient = await _context.FavoriteRecipients
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recipient == null) return NotFound();

        _context.FavoriteRecipients.Remove(recipient);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
