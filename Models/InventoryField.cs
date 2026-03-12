using System;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models
{
    public class InventoryField
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InventoryId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Type { get; set; } = "SingleLine"; // single line, multi line, numeric, document, boolean

        public bool IsDisplay { get; set; } = false;

        public bool IsRequired { get; set; } = false;

        // Navigation properties
        public Inventory? Inventory { get; set; }
        public ICollection<InventoryFieldData> FieldData { get; set; } =
            new List<InventoryFieldData>();
    }
}
