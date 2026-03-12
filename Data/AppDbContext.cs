using invex_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace invex_api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryAccess> InventoryAccesses { get; set; }
    public DbSet<InventoryData> InventoryData { get; set; }
    public DbSet<InventoryField> InventoryFields { get; set; }
    public DbSet<InventoryFieldData> InventoryFieldData { get; set; }
    public DbSet<InventoryPost> InventoryPosts { get; set; }
    public DbSet<InventoryPostLike> InventoryPostLikes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().Property(u => u.AccessRole).HasConversion<string>();

        // Inventory -> Owner relationship
        builder
            .Entity<Inventory>()
            .HasOne(i => i.Owner)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryAccess -> User relationship
        builder
            .Entity<InventoryAccess>()
            .HasOne(ia => ia.User)
            .WithMany(u => u.InventoryAccesses)
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // InventoryAccess -> Inventory relationship
        builder
            .Entity<InventoryAccess>()
            .HasOne(ia => ia.Inventory)
            .WithMany(i => i.Accesses)
            .HasForeignKey(ia => ia.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.Entity<Inventory>().HasIndex(i => i.OwnerId);
        builder.Entity<Inventory>().HasIndex(i => i.Id);

        builder.Entity<InventoryAccess>().HasIndex(ia => new { ia.UserId, ia.InventoryId });
        // Inventory -> InventoryData relationship
        builder
            .Entity<InventoryData>()
            .HasOne(id => id.Inventory)
            .WithMany(i => i.Items)
            .HasForeignKey(id => id.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inventory -> InventoryField relationship
        builder
            .Entity<InventoryField>()
            .HasOne(f => f.Inventory)
            .WithMany(i => i.Fields)
            .HasForeignKey(f => f.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // InventoryData -> InventoryFieldData relationship
        builder
            .Entity<InventoryFieldData>()
            .HasOne(fd => fd.InventoryData)
            .WithMany(d => d.FieldData)
            .HasForeignKey(fd => fd.InventoryDataId)
            .OnDelete(DeleteBehavior.Cascade);

        // InventoryField -> InventoryFieldData relationship
        builder
            .Entity<InventoryFieldData>()
            .HasOne(fd => fd.CustomField)
            .WithMany(f => f.FieldData)
            .HasForeignKey(fd => fd.CustomFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for InventoryData
        builder.Entity<InventoryData>().HasIndex(d => d.InventoryId);
        builder.Entity<InventoryData>().HasIndex(d => d.CreatedAt);

        // Indexes for InventoryField
        builder.Entity<InventoryField>().HasIndex(f => f.InventoryId);

        // Indexes for InventoryFieldData
        builder.Entity<InventoryFieldData>().HasIndex(fd => fd.InventoryDataId);
        builder.Entity<InventoryFieldData>().HasIndex(fd => fd.CustomFieldId);
        builder
            .Entity<InventoryFieldData>()
            .HasIndex(fd => new { fd.InventoryDataId, fd.CustomFieldId })
            .IsUnique();

        // -------------------------
        // InventoryPost mappings
        // -------------------------

        builder
            .Entity<InventoryPost>()
            .HasOne(p => p.Inventory)
            .WithMany(i => i.Posts)
            .HasForeignKey(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<InventoryPost>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InventoryPost>().HasIndex(p => p.InventoryId);
        builder.Entity<InventoryPost>().HasIndex(p => p.CreatedAt);

        // -------------------------
        // InventoryPostLike mappings
        // -------------------------

        builder.Entity<InventoryPostLike>().HasKey(l => new { l.PostId, l.UserId });

        builder
            .Entity<InventoryPostLike>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<InventoryPostLike>()
            .HasOne(l => l.User)
            .WithMany(u => u.PostLikes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
