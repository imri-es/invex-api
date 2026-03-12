using System;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models;

public class InventoryPostLike
{
    [Required]
    public Guid PostId { get; set; }
    public InventoryPost Post { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
