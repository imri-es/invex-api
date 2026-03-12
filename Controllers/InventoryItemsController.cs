using System.Security.Claims;
using invex_api.Data;
using invex_api.DTOs;
using invex_api.Models;
using invex_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace invex_api.Controllers
{
    [Route("api/inventories/{inventoryId}/items")]
    [ApiController]
    [Authorize]
    public class InventoryItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CustomIdGeneratorService _customIdGenerator;

        public InventoryItemsController(
            AppDbContext context,
            CustomIdGeneratorService customIdGenerator
        )
        {
            _context = context;
            _customIdGenerator = customIdGenerator;
        }

        // GET: api/inventories/{inventoryId}/items
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDto<InventoryItemResponseDto>>> GetItems(
            Guid inventoryId,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var hasAccess =
                inventory.OwnerId == userId || inventory.Accesses.Any(a => a.UserId == userId);
            if (!hasAccess)
                return Forbid();

            var query = _context
                .InventoryData.Where(d => d.InventoryId == inventoryId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(d =>
                    d.Name.ToLower().Contains(lowerSearch)
                    || (d.Description != null && d.Description.ToLower().Contains(lowerSearch))
                );
            }

            var totalCount = await query.CountAsync();

            query = query.Include(d => d.FieldData).ThenInclude(fd => fd.CustomField);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.StartsWith("custom_"))
                {
                    if (Guid.TryParse(sortBy.Substring("custom_".Length), out var fieldId))
                    {
                        var field = inventory.Fields.FirstOrDefault(f => f.Id == fieldId);
                        if (field != null)
                        {
                            if (field.Type == "Numeric")
                            {
                                query = sortDesc
                                    ? query.OrderByDescending(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueNumeric
                                    )
                                    : query.OrderBy(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueNumeric
                                    );
                            }
                            else if (field.Type == "Boolean")
                            {
                                query = sortDesc
                                    ? query.OrderByDescending(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueBoolean
                                    )
                                    : query.OrderBy(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueBoolean
                                    );
                            }
                            else
                            {
                                query = sortDesc
                                    ? query.OrderByDescending(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueString
                                    )
                                    : query.OrderBy(d =>
                                        d.FieldData.FirstOrDefault(fd =>
                                            fd.CustomFieldId == fieldId
                                        )!.ValueString
                                    );
                            }
                        }
                    }
                }
                else
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            query = sortDesc
                                ? query.OrderByDescending(d => d.Name)
                                : query.OrderBy(d => d.Name);
                            break;
                        case "description":
                            query = sortDesc
                                ? query.OrderByDescending(d => d.Description)
                                : query.OrderBy(d => d.Description);
                            break;
                        case "quantity":
                            query = sortDesc
                                ? query.OrderByDescending(d => d.Quantity)
                                : query.OrderBy(d => d.Quantity);
                            break;
                        case "customid":
                            query = sortDesc
                                ? query.OrderByDescending(d => d.CustomID)
                                : query.OrderBy(d => d.CustomID);
                            break;
                        default:
                            query = sortDesc
                                ? query.OrderByDescending(d => d.CreatedAt)
                                : query.OrderBy(d => d.CreatedAt);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(d => d.CreatedAt);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new InventoryItemResponseDto
                {
                    Id = d.Id,
                    CustomID = d.CustomID,
                    InventoryId = d.InventoryId,
                    Name = d.Name,
                    Description = d.Description,
                    Image = d.Image,
                    Quantity = d.Quantity,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    FieldData = d
                        .FieldData.Select(fd => new InventoryFieldDataResponseDto
                        {
                            Id = fd.Id,
                            CustomFieldId = fd.CustomFieldId,
                            CustomFieldName = fd.CustomField != null ? fd.CustomField.Name : "",
                            CustomFieldType = fd.CustomField != null ? fd.CustomField.Type : "",
                            ValueString = fd.ValueString,
                            ValueNumeric = fd.ValueNumeric,
                            ValueBoolean = fd.ValueBoolean,
                        })
                        .ToList(),
                })
                .ToListAsync();

            var response = new PaginatedResponseDto<InventoryItemResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };

            return Ok(response);
        }

        // GET: api/inventories/{inventoryId}/items/{itemId}
        [HttpGet("{itemId}")]
        public async Task<ActionResult<InventoryItemResponseDto>> GetItem(
            Guid inventoryId,
            Guid itemId
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var hasAccess =
                inventory.OwnerId == userId || inventory.Accesses.Any(a => a.UserId == userId);
            if (!hasAccess)
                return Forbid();

            var dto = await _context
                .InventoryData.Where(d => d.Id == itemId && d.InventoryId == inventoryId)
                .Select(item => new InventoryItemResponseDto
                {
                    Id = item.Id,
                    CustomID = item.CustomID,
                    InventoryId = item.InventoryId,
                    Name = item.Name,
                    Description = item.Description,
                    Image = item.Image,
                    Quantity = item.Quantity,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    FieldData = item
                        .FieldData.Select(fd => new InventoryFieldDataResponseDto
                        {
                            Id = fd.Id,
                            CustomFieldId = fd.CustomFieldId,
                            CustomFieldName = fd.CustomField.Name,
                            CustomFieldType = fd.CustomField.Type,
                            ValueString = fd.ValueString,
                            ValueNumeric = fd.ValueNumeric,
                            ValueBoolean = fd.ValueBoolean,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync();

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        // POST: api/inventories/{inventoryId}/items
        [HttpPost]
        public async Task<ActionResult<InventoryItemResponseDto>> CreateItem(
            Guid inventoryId,
            CreateInventoryItemDto createDto
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var hasAccess =
                inventory.OwnerId == userId
                || inventory.Accesses.Any(a =>
                    a.UserId == userId && (a.AccessType == "Admin" || a.AccessType == "Write")
                );
            if (!hasAccess)
                return Forbid();

            // Validate required fields
            var providedFieldIds =
                createDto.FieldData?.Select(f => f.CustomFieldId).ToList() ?? new List<Guid>();
            var requiredFields = inventory.Fields.Where(f => f.IsRequired).ToList();

            foreach (var reqField in requiredFields)
            {
                if (!providedFieldIds.Contains(reqField.Id))
                {
                    // Check if missing or check if provided but null
                    return BadRequest($"Required field '{reqField.Name}' is missing.");
                }

                var providedField = createDto.FieldData!.First(f => f.CustomFieldId == reqField.Id);

                bool isEmpty = false;
                if (
                    reqField.Type == "SingleLine"
                    || reqField.Type == "MultiLine"
                    || reqField.Type == "Document"
                )
                {
                    isEmpty = string.IsNullOrWhiteSpace(providedField.ValueString);
                }
                else if (reqField.Type == "Numeric")
                {
                    isEmpty = providedField.ValueNumeric == null;
                }
                else if (reqField.Type == "Boolean")
                {
                    isEmpty = providedField.ValueBoolean == null;
                }

                if (isEmpty)
                {
                    return BadRequest($"Required field '{reqField.Name}' cannot be empty.");
                }
            }

            var item = new InventoryData
            {
                InventoryId = inventoryId,
                Name = createDto.Name,
                Description = createDto.Description,
                Image = createDto.Image,
                Quantity = createDto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Generate CustomID from mask if available
            if (!string.IsNullOrEmpty(inventory.CustomIdMask))
            {
                item.CustomID = _customIdGenerator.GenerateCustomId(
                    inventory.CustomIdMask,
                    inventory.NumberOfRecords
                );
            }

            _context.InventoryData.Add(item);

            // Add custom field data if provided
            if (createDto.FieldData != null)
            {
                foreach (var fd in createDto.FieldData)
                {
                    _context.InventoryFieldData.Add(
                        new InventoryFieldData
                        {
                            InventoryDataId = item.Id,
                            CustomFieldId = fd.CustomFieldId,
                            ValueString = fd.ValueString,
                            ValueNumeric = fd.ValueNumeric,
                            ValueBoolean = fd.ValueBoolean,
                        }
                    );
                }
            }

            // Increment record count
            inventory.NumberOfRecords += 1;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Re-fetch with includes for the response
            var created = await _context
                .InventoryData.Include(d => d.FieldData)
                    .ThenInclude(fd => fd.CustomField)
                .FirstAsync(d => d.Id == item.Id);

            var dto = new InventoryItemResponseDto
            {
                Id = created.Id,
                CustomID = created.CustomID,
                InventoryId = created.InventoryId,
                Name = created.Name,
                Description = created.Description,
                Image = created.Image,
                Quantity = created.Quantity,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                FieldData = created
                    .FieldData.Select(fd => new InventoryFieldDataResponseDto
                    {
                        Id = fd.Id,
                        CustomFieldId = fd.CustomFieldId,
                        CustomFieldName = fd.CustomField?.Name ?? "",
                        CustomFieldType = fd.CustomField?.Type ?? "",
                        ValueString = fd.ValueString,
                        ValueNumeric = fd.ValueNumeric,
                        ValueBoolean = fd.ValueBoolean,
                    })
                    .ToList(),
            };

            return CreatedAtAction(nameof(GetItem), new { inventoryId, itemId = item.Id }, dto);
        }

        // DELETE: api/inventories/{inventoryId}/items/{itemId}
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteItem(Guid inventoryId, Guid itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var hasAccess =
                inventory.OwnerId == userId
                || inventory.Accesses.Any(a =>
                    a.UserId == userId && (a.AccessType == "Admin" || a.AccessType == "Write")
                );
            if (!hasAccess)
                return Forbid();

            var item = await _context.InventoryData.FirstOrDefaultAsync(d =>
                d.Id == itemId && d.InventoryId == inventoryId
            );

            if (item == null)
                return NotFound();

            _context.InventoryData.Remove(item);

            inventory.NumberOfRecords = Math.Max(0, inventory.NumberOfRecords - 1);
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
