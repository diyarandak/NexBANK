using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int userId, string role);
}
