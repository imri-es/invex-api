using System;

namespace invex_api.DTOs
{
    public class CreateInventoryDto
    {
        public required string Name { get; set; }
        public string Visibility { get; set; } = "Private";
    }

    public class InventoryAccessDto
    {
        public Guid? Id { get; set; }
        public required string UserId { get; set; }
        public required string AccessType { get; set; }
    }

    public class UpdateInventoryDto
    {
        public string? Name { get; set; }
        public string? Visibility { get; set; }
        public string? CustomIdMask { get; set; }
        public List<CustomFieldDto>? Fields { get; set; }
        public List<InventoryAccessDto>? Accesses { get; set; }
    }

    public class InventoryResponseDto
    {
        public Guid Id { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public int NumberOfRecords { get; set; }
        public string CustomIdMask { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
