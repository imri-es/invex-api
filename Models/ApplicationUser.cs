using invex_api.Enums;
using Microsoft.AspNetCore.Identity;

namespace invex_api.Models;

public class ApplicationUser : IdentityUser
{
    public UserRole AccessRole { get; set; } = UserRole.Newbie;
    public DateTime? LastLogin { get; set; }
    public required string FullName { get; set; }
    public string? SocialNetworkType { get; set; }
    public string? SocialNetworkId { get; set; }
}
