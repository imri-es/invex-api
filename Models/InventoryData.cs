using System;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models
{
    public class InventoryData
    {
        [Key]
        public Guid Id { get; set; }

        public string? CustomID { get; set; }

        [Required]
        public Guid InventoryId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Image { get; set; }

        public int Quantity { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Inventory? Inventory { get; set; }
        public ICollection<InventoryFieldData> FieldData { get; set; } =
            new List<InventoryFieldData>();
    }
}
