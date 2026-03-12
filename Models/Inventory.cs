using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models
{
    public class Inventory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [Required]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Visibility { get; set; } = "Private";

        public int NumberOfRecords { get; set; } = 0;

        public string CustomIdMask { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser? Owner { get; set; }
        public ICollection<InventoryAccess> Accesses { get; set; } = new List<InventoryAccess>();
        public ICollection<InventoryData> Items { get; set; } = new List<InventoryData>();
        public ICollection<InventoryField> Fields { get; set; } = new List<InventoryField>();
        public ICollection<InventoryPost> Posts { get; set; } = new List<InventoryPost>();
    }
}
