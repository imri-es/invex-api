using System.ComponentModel.DataAnnotations;

namespace invex_api.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string FullName { get; set; }
}

public class AuthSocialDto
{
    [Required]
    public required string Token { get; set; }

    [Required]
    public required string Provider { get; set; } // "google", "facebook", etc.
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
