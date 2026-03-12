using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models;

public class InventoryPost
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InventoryPostLike> Likes { get; set; } = new List<InventoryPostLike>();
}
