using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await _profileService.GetProfileAsync(GetUserId());
        if (user == null) return NotFound();
        return Ok(user);
    }

    public class UpdateProfileDto { public string FullName { get; set; } = ""; public string Email { get; set; } = ""; }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var success = await _profileService.UpdateProfileAsync(GetUserId(), dto.FullName, dto.Email);
        return success ? Ok(new { Message = "Profil güncellendi" }) : BadRequest();
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications() => Ok(await _profileService.GetNotificationsAsync(GetUserId()));

    [HttpPost("notifications/{id}/read")]
    public async Task<IActionResult> ReadNotification(int id)
    {
        await _profileService.MarkNotificationAsReadAsync(id, GetUserId());
        return Ok();
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets() => Ok(await _profileService.GetTicketsAsync(GetUserId()));

    public class CreateTicketDto { public string Subject { get; set; } = ""; public string Message { get; set; } = ""; }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var ticket = await _profileService.CreateTicketAsync(GetUserId(), dto.Subject, dto.Message);
        return Ok(new { Message = "Destek talebi oluşturuldu.", Ticket = ticket });
    }
}
