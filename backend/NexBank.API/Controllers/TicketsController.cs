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
public class TicketsController : ControllerBase
{
    private readonly NexBankDbContext _context;

    public TicketsController(NexBankDbContext context)
    {
        _context = context;
    }

    // Müşterinin kendi taleplerini çekmesi
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var tickets = await _context.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        return Ok(tickets);
    }

    // Personelin şubesindeki talepleri çekmesi
    [HttpGet("staff/branch-tickets")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetBranchTickets()
    {
        var staffBranch = User.FindFirst("Branch")?.Value;
        var tickets = await _context.Tickets
            .Include(t => t.User)
            .Where(t => t.User.Branch == staffBranch)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();
        return Ok(tickets);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] NexBank.Application.DTOs.CreateTicketDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var ticket = new Ticket
        {
            UserId = userId,
            Subject = dto.Subject,
            Message = dto.Message,
            Priority = Enum.TryParse<TicketPriority>(dto.Priority, out var p) ? p : TicketPriority.Normal,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return Ok(ticket);
    }

    [HttpPut("staff/respond/{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> RespondToTicket(int id, [FromBody] string response)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.StaffResponse = response;
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ticket);
    }
}
