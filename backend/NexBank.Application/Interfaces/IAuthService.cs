using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> CustomerLoginAsync(CustomerLoginRequestDto request);
    Task<LoginResponseDto?> StaffLoginAsync(StaffLoginRequestDto request);
    Task<UserDto> RegisterCustomerAsync(RegisterRequestDto request);
    Task<UserDto> RegisterStaffAsync(StaffRegisterRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
