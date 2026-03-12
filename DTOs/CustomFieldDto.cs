using System;

namespace invex_api.DTOs
{
    public class CustomFieldDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "SingleLine";
        public bool IsDisplay { get; set; } = false;
        public bool IsRequired { get; set; } = false;
    }
}
