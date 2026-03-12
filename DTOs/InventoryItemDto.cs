using System;

namespace invex_api.DTOs
{
    public class InventoryFieldDataDto
    {
        public Guid CustomFieldId { get; set; }
        public string? ValueString { get; set; }
        public decimal? ValueNumeric { get; set; }
        public bool? ValueBoolean { get; set; }
    }

    public class CreateInventoryItemDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; } = 0;
        public List<InventoryFieldDataDto>? FieldData { get; set; }
    }

    public class InventoryFieldDataResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomFieldId { get; set; }
        public string CustomFieldName { get; set; } = string.Empty;
        public string CustomFieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? ValueString { get; set; }
        public decimal? ValueNumeric { get; set; }
        public bool? ValueBoolean { get; set; }
    }

    public class InventoryItemResponseDto
    {
        public Guid Id { get; set; }
        public string? CustomID { get; set; }
        public Guid InventoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<InventoryFieldDataResponseDto> FieldData { get; set; } = new();
    }
}
