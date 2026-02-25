using invex_api.Enums;

namespace invex_api.DTOs;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public UserRole AccessRole { get; set; }
}
