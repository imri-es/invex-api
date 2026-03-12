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

    // Navigation properties for Inventories
    public ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
    public ICollection<InventoryAccess> InventoryAccesses { get; set; } =
        new List<InventoryAccess>();
    public ICollection<InventoryPost> Posts { get; set; } = new List<InventoryPost>();
    public ICollection<InventoryPostLike> PostLikes { get; set; } = new List<InventoryPostLike>();
}
