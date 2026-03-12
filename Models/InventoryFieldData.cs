using System;
using System.ComponentModel.DataAnnotations;

namespace invex_api.Models
{
    public class InventoryFieldData
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InventoryDataId { get; set; }

        [Required]
        public Guid CustomFieldId { get; set; }

        public string? ValueString { get; set; }

        public decimal? ValueNumeric { get; set; }

        public bool? ValueBoolean { get; set; }

        // Navigation properties
        public InventoryData? InventoryData { get; set; }
        public InventoryField? CustomField { get; set; }
    }
}
