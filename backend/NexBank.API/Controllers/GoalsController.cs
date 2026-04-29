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
public class GoalsController : ControllerBase
{
    private readonly NexBankDbContext _context;

    public GoalsController(NexBankDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
        return Ok(goals);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] SavingGoal goal)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        goal.UserId = userId;
        goal.CreatedAt = DateTime.UtcNow;
        goal.CurrentAmount = 0;
        goal.IsCompleted = false;

        _context.SavingGoals.Add(goal);
        await _context.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpPost("{id}/add-money")]
    public async Task<IActionResult> AddMoney(int id, [FromBody] decimal amount)
    {
        var goal = await _context.SavingGoals.FindAsync(id);
        if (goal == null) return NotFound();

        goal.CurrentAmount += amount;
        if (goal.CurrentAmount >= goal.TargetAmount)
            goal.IsCompleted = true;

        await _context.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var goal = await _context.SavingGoals.FindAsync(id);
        if (goal == null) return NotFound();

        _context.SavingGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
