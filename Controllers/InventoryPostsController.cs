using System.Security.Claims;
using invex_api.Data;
using invex_api.DTOs;
using invex_api.Enums;
using invex_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace invex_api.Controllers;

[Route("api/inventories/{inventoryId}/posts")]
[ApiController]
[Authorize]
public class InventoryPostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventoryPostsController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<InventoryPostDto>>> GetPosts(
        Guid inventoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "date_desc"
    )
    {
        var inventory = await _context.Inventories.FindAsync(inventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        var currentUserId = GetCurrentUserId();

        // Authorization check - User must be owner or have access
        var hasAccess = _context.InventoryAccesses.Any(a =>
            a.InventoryId == inventoryId && a.UserId == currentUserId
        );
        if (inventory.OwnerId != currentUserId && !hasAccess && inventory.Visibility == "Private")
        {
            // Let's assume if it's public they can view it. If private, they need access or be owner.
            var userClaimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userClaimRole != UserRole.Admin.ToString() && inventory.Visibility != "Public")
            {
                return Forbid("You do not have access to this inventory's posts.");
            }
        }

        var query = _context
            .InventoryPosts.Include(p => p.User)
            .Include(p => p.Likes)
            .Where(p => p.InventoryId == inventoryId)
            .AsQueryable();

        // Sorting
        query = sortBy.ToLower() switch
        {
            "date_asc" => query.OrderBy(p => p.CreatedAt),
            "likes_desc" => query
                .OrderByDescending(p => p.Likes.Count)
                .ThenByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt), // Default: newest first
        };

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new InventoryPostDto
            {
                Id = p.Id,
                InventoryId = p.InventoryId,
                UserId = p.UserId,
                FullName = p.User.FullName,
                UserName = p.User.UserName!,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                LikesCount = p.Likes.Count,
                IsLikedByCurrentUser =
                    currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
            })
            .ToListAsync();

        return Ok(
            new PaginatedResponseDto<InventoryPostDto>
            {
                Items = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            }
        );
    }

    [HttpPost]
    public async Task<ActionResult<InventoryPostDto>> CreatePost(
        Guid inventoryId,
        [FromBody] CreateInventoryPostDto dto
    )
    {
        var inventory = await _context.Inventories.FindAsync(inventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        // Re-check auth logic before creating
        var hasAccess = _context.InventoryAccesses.Any(a =>
            a.InventoryId == inventoryId && a.UserId == currentUserId
        );
        var userClaimRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (
            inventory.OwnerId != currentUserId
            && !hasAccess
            && userClaimRole != UserRole.Admin.ToString()
        )
        {
            return Forbid("You do not have permission to post in this inventory.");
        }

        var post = new InventoryPost
        {
            Id = Guid.NewGuid(),
            InventoryId = inventoryId,
            UserId = currentUserId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
        };

        _context.InventoryPosts.Add(post);
        await _context.SaveChangesAsync();

        // Reload to get User details
        var createdPost = await _context
            .InventoryPosts.Include(p => p.User)
            .FirstAsync(p => p.Id == post.Id);

        return CreatedAtAction(
            nameof(GetPosts),
            new { inventoryId = inventory.Id },
            new InventoryPostDto
            {
                Id = createdPost.Id,
                InventoryId = createdPost.InventoryId,
                UserId = createdPost.UserId,
                FullName = createdPost.User.FullName,
                UserName = createdPost.User.UserName!,
                Content = createdPost.Content,
                CreatedAt = createdPost.CreatedAt,
                LikesCount = 0,
                IsLikedByCurrentUser = false,
            }
        );
    }

    [HttpPut("{postId}/like")]
    public async Task<IActionResult> ToggleLike(Guid inventoryId, Guid postId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var post = await _context
            .InventoryPosts.Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == postId && p.InventoryId == inventoryId);

        if (post == null)
            return NotFound("Post not found");

        var existingLike = post.Likes.FirstOrDefault(l => l.UserId == currentUserId);

        if (existingLike != null)
        {
            _context.InventoryPostLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Ok(new { liked = false, likesCount = post.Likes.Count - 1 });
        }
        else
        {
            var newLike = new InventoryPostLike { PostId = postId, UserId = currentUserId };
            _context.InventoryPostLikes.Add(newLike);
            await _context.SaveChangesAsync();
            return Ok(new { liked = true, likesCount = post.Likes.Count + 1 });
        }
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost(Guid inventoryId, Guid postId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var post = await _context
            .InventoryPosts.Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == postId && p.InventoryId == inventoryId);

        if (post == null)
            return NotFound("Post not found");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isAdmin = userRole == UserRole.Admin.ToString();
        bool isOwner = post.Inventory.OwnerId == currentUserId;
        bool isAuthor = post.UserId == currentUserId;

        if (!isAdmin && !isOwner && !isAuthor)
        {
            return Forbid("Only the author, inventory owner, or an admin can delete this post.");
        }

        _context.InventoryPosts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
