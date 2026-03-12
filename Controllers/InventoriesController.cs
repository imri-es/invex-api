using System.Security.Claims;
using invex_api.Data;
using invex_api.DTOs;
using invex_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace invex_api.Controllers
{
    [Route("api/inventories")]
    [ApiController]
    [Authorize]
    public class InventoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryResponseDto>>> GetInventories(
            [FromQuery] bool owned = true,
            [FromQuery] bool hasAccess = true
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get inventories based on query parameters
            var query = _context.Inventories.AsQueryable();

            if (owned && !hasAccess)
            {
                query = query.Where(i => i.OwnerId == userId);
            }
            else if (hasAccess && !owned)
            {
                query = query.Where(i => i.Accesses.Any(a => a.UserId == userId));
            }
            else if (owned && hasAccess)
            {
                query = query.Where(i =>
                    i.OwnerId == userId || i.Accesses.Any(a => a.UserId == userId)
                );
            }
            else
            {
                // If both are false, return empty list
                return Ok(new List<InventoryResponseDto>());
            }

            var inventories = await query
                .Select(i => new InventoryResponseDto
                {
                    Id = i.Id,
                    OwnerId = i.OwnerId,
                    OwnerEmail = i.OwnerEmail,
                    Name = i.Name,
                    Visibility = i.Visibility,
                    NumberOfRecords = i.NumberOfRecords,
                    CustomIdMask = i.CustomIdMask,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                })
                .ToListAsync();

            return Ok(inventories);
        }

        // GET: api/Inventories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryResponseDto>> GetInventory(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            // Check access
            if (inventory.OwnerId != userId && !inventory.Accesses.Any(a => a.UserId == userId))
            {
                return Forbid();
            }

            var dto = new InventoryResponseDto
            {
                Id = inventory.Id,
                OwnerId = inventory.OwnerId,
                OwnerEmail = inventory.OwnerEmail,
                Name = inventory.Name,
                Visibility = inventory.Visibility,
                NumberOfRecords = inventory.NumberOfRecords,
                CustomIdMask = inventory.CustomIdMask,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt,
            };

            return Ok(dto);
        }

        // POST: api/Inventories
        [HttpPost]
        public async Task<ActionResult<InventoryResponseDto>> CreateInventory(
            CreateInventoryDto createDto
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var inventory = new Inventory
            {
                OwnerId = userId,
                OwnerEmail = userEmail,
                Name = createDto.Name,
                Visibility = createDto.Visibility,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Inventories.Add(inventory);

            // Add owner as Admin access implicitly or explicitly here if needed.
            // Currently, Owner check handles it implicitly.

            await _context.SaveChangesAsync();

            var dto = new InventoryResponseDto
            {
                Id = inventory.Id,
                OwnerId = inventory.OwnerId,
                OwnerEmail = inventory.OwnerEmail,
                Name = inventory.Name,
                Visibility = inventory.Visibility,
                NumberOfRecords = inventory.NumberOfRecords,
                CustomIdMask = inventory.CustomIdMask,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt,
            };

            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Id }, dto);
        }

        // PUT: api/Inventories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(Guid id, UpdateInventoryDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            // Allow Owner or someone with "Admin" access
            var hasAccess =
                inventory.OwnerId == userId
                || inventory.Accesses.Any(a => a.UserId == userId && a.AccessType == "Admin");

            if (!hasAccess)
            {
                return Forbid();
            }

            if (updateDto.Name != null)
                inventory.Name = updateDto.Name;
            if (updateDto.Visibility != null)
                inventory.Visibility = updateDto.Visibility;
            if (updateDto.CustomIdMask != null)
                inventory.CustomIdMask = updateDto.CustomIdMask;

            // Update Fields if provided
            if (updateDto.Fields != null)
            {
                var incomingFieldIds = updateDto
                    .Fields.Where(f => f.Id.HasValue)
                    .Select(f => f.Id.Value)
                    .ToList();
                var fieldsToRemove = inventory
                    .Fields.Where(f => !incomingFieldIds.Contains(f.Id))
                    .ToList();
                foreach (var field in fieldsToRemove)
                {
                    _context.InventoryFields.Remove(field);
                }

                foreach (var dto in updateDto.Fields)
                {
                    if (dto.Id.HasValue)
                    {
                        var existing = inventory.Fields.FirstOrDefault(f => f.Id == dto.Id.Value);
                        if (existing != null)
                        {
                            existing.Name = dto.Name;
                            existing.Description = dto.Description;
                            existing.Type = dto.Type;
                            existing.IsDisplay = dto.IsDisplay;
                            existing.IsRequired = dto.IsRequired;
                        }
                    }
                    else
                    {
                        inventory.Fields.Add(
                            new InventoryField
                            {
                                InventoryId = id,
                                Name = dto.Name,
                                Description = dto.Description,
                                Type = dto.Type,
                                IsDisplay = dto.IsDisplay,
                                IsRequired = dto.IsRequired,
                            }
                        );
                    }
                }
            }

            // Update Accesses if provided
            if (updateDto.Accesses != null)
            {
                var incomingAccessIds = updateDto
                    .Accesses.Where(a => a.Id.HasValue)
                    .Select(a => a.Id.Value)
                    .ToList();
                var accessesToRemove = inventory
                    .Accesses.Where(a => !incomingAccessIds.Contains(a.Id))
                    .ToList();
                foreach (var access in accessesToRemove)
                {
                    _context.InventoryAccesses.Remove(access);
                }

                foreach (var dto in updateDto.Accesses)
                {
                    if (dto.Id.HasValue)
                    {
                        var existing = inventory.Accesses.FirstOrDefault(a => a.Id == dto.Id.Value);
                        if (existing != null)
                        {
                            existing.UserId = dto.UserId;
                            existing.AccessType = dto.AccessType;
                        }
                    }
                    else
                    {
                        inventory.Accesses.Add(
                            new InventoryAccess
                            {
                                InventoryId = id,
                                UserId = dto.UserId,
                                AccessType = dto.AccessType,
                            }
                        );
                    }
                }
            }

            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Inventories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var inventory = await _context.Inventories.FindAsync(id);

            if (inventory == null)
            {
                return NotFound();
            }

            // Only Owner can delete for now
            if (inventory.OwnerId != userId)
            {
                return Forbid();
            }

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/inventories/{inventoryId}/fields
        [HttpGet("{inventoryId}/fields")]
        public async Task<ActionResult<IEnumerable<CustomFieldDto>>> GetInventoryFields(
            Guid inventoryId
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

            var dtos = inventory
                .Fields.Select(f => new CustomFieldDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Type = f.Type,
                    IsDisplay = f.IsDisplay,
                    IsRequired = f.IsRequired,
                })
                .ToList();

            return Ok(dtos);
        }

        // GET: api/inventories/{inventoryId}/accesses
        [HttpGet("{inventoryId}/accesses")]
        public async Task<IActionResult> GetAccesses(Guid inventoryId)
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

            var accessIds = inventory.Accesses.Select(a => a.UserId).ToList();
            var users = await _context
                .Users.Where(u => accessIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var result = inventory
                .Accesses.Select(a => new
                {
                    a.Id,
                    a.UserId,
                    Email = users.ContainsKey(a.UserId) ? users[a.UserId].Email : "",
                    FullName = users.ContainsKey(a.UserId) ? users[a.UserId].FullName : "",
                    a.AccessType,
                })
                .ToList();

            return Ok(result);
        }

        // POST: api/inventories/{inventoryId}/accesses
        [HttpPost("{inventoryId}/accesses")]
        public async Task<IActionResult> GrantAccess(
            Guid inventoryId,
            [FromBody] InventoryAccessDto dto
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var isOwnerOrAdmin =
                inventory.OwnerId == userId
                || inventory.Accesses.Any(a => a.UserId == userId && a.AccessType == "Admin");
            if (!isOwnerOrAdmin)
                return Forbid();

            // Prevent duplicate access
            if (inventory.Accesses.Any(a => a.UserId == dto.UserId))
                return BadRequest("User already has access to this inventory.");

            var access = new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = dto.UserId,
                AccessType = dto.AccessType,
            };

            _context.InventoryAccesses.Add(access);
            await _context.SaveChangesAsync();

            // Fetch user details for the response
            var user = await _context.Users.FindAsync(dto.UserId);

            return Ok(
                new
                {
                    access.Id,
                    access.UserId,
                    Email = user?.Email ?? "",
                    FullName = user?.FullName ?? "",
                    access.AccessType,
                }
            );
        }

        // DELETE: api/inventories/{inventoryId}/accesses
        [HttpDelete("{inventoryId}/accesses")]
        public async Task<IActionResult> BulkDeleteAccesses(
            Guid inventoryId,
            [FromBody] List<Guid> accessIds
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inventory = await _context
                .Inventories.Include(i => i.Accesses)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return NotFound();

            var isOwnerOrAdmin =
                inventory.OwnerId == userId
                || inventory.Accesses.Any(a => a.UserId == userId && a.AccessType == "Admin");
            if (!isOwnerOrAdmin)
                return Forbid();

            var toRemove = inventory.Accesses.Where(a => accessIds.Contains(a.Id)).ToList();

            foreach (var access in toRemove)
            {
                _context.InventoryAccesses.Remove(access);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
