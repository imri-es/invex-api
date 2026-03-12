using System;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models
{
    public class InventoryAccess
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public Guid InventoryId { get; set; }

        [Required]
        public string AccessType { get; set; } = "Read"; // e.g., "Read", "Write", "Admin"

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Inventory? Inventory { get; set; }
    }
}
