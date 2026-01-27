using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Claims;
using System.Linq;

namespace RentACar.Infrastructure.Data;

public partial class RentACarDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RentACarDbContext(DbContextOptions<RentACarDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
    public virtual DbSet<BlackList> BlackLists { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<Car> Cars { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<CreditCard> CreditCards { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerCreditCard> CustomerCreditCards { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
    public virtual DbSet<Promocode> Promocodes { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<CustomerRating> CustomerRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<BlackList>(entity =>
        {
            entity.HasKey(e => e.BlacklistId).HasName("PK_BlackList_1");

            entity.HasOne(d => d.EmployeeDoneBlacklist).WithMany(p => p.BlackLists)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlackList_Employees");

            entity.HasOne(d => d.User).WithMany(p => p.BlackLists)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlackList_AspNetUsers1");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.IsBookedByEmployee).HasDefaultValue(false);

            entity.HasOne(d => d.Car).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Cars1");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Customers1");

            entity.HasOne(d => d.Employeebooker).WithMany(p => p.Bookings)
                .HasConstraintName("FK_Bookings_Employees");

             entity.HasOne(d => d.Promocode).WithMany(p => p.Bookings)
                .HasConstraintName("FK_Bookings_Promocodes1");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.Cars)
                .HasConstraintName("FK_Cars_Categories1");
        });

        modelBuilder.Entity<CreditCard>(entity =>
        {
            entity.HasKey(e => e.CreditCardId).HasName("PK_CreditCard_1");

            entity.Property(e => e.CardHolderName).IsFixedLength();
            entity.Property(e => e.Cvv).IsFixedLength();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Customers_1");

            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customers_AspNetUsers");
        });

        modelBuilder.Entity<CustomerCreditCard>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CreditCardId });

            entity.HasOne(e => e.User)
                .WithMany(c => c.CustomerCreditCards)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerCreditCard_Customers");

            entity.HasOne(e => e.CreditCard)
                .WithMany(cc => cc.CustomerCreditCards)
                .HasForeignKey(e => e.CreditCardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerCreditCard_CreditCard");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(d => d.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Payments_Bookings");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditLog> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLog>();
        
        var user = _httpContextAccessor?.HttpContext?.User;
        var userName = user?.Identity?.Name ?? "System"; 
        
        if (user?.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(userName))
        {
             userName = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown User";
        }

        var userRole = "Unknown";
        if (user != null)
        {
            var roles = user.FindAll(ClaimTypes.Role);
            if (roles.Any())
            {
                userRole = string.Join(", ", roles.Select(r => r.Value));
            }
        }
        
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                ActorName = userName,
                ActorRole = userRole,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Action = entry.State.ToString(),
                Entity = entry.Entity.GetType().Name,
                Status = "Success",
                TargetType = entry.Entity.GetType().Name,
                Outcome = "Success"
            };

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (primaryKey != null && primaryKey.CurrentValue != null)
            {
                auditEntry.EntityId = primaryKey.CurrentValue.ToString();
                auditEntry.TargetId = primaryKey.CurrentValue.ToString();
            }

            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();
            var changes = new List<string>();

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.IsTemporary) continue;

                var originalVal = property.OriginalValue;
                var currentVal = property.CurrentValue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = MaskSensitiveData(propertyName, currentVal);
                        changes.Add($"{propertyName}: {FormatValue(currentVal)}");
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = MaskSensitiveData(propertyName, originalVal);
                        changes.Add($"{propertyName}: {FormatValue(originalVal)}");
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            // Only log if effectively different
                            var strOriginal = originalVal?.ToString();
                            var strCurrent = currentVal?.ToString();
                            
                            if (strOriginal != strCurrent)
                            {
                                oldValues[propertyName] = MaskSensitiveData(propertyName, originalVal);
                                newValues[propertyName] = MaskSensitiveData(propertyName, currentVal);
                                changes.Add($"{propertyName}: {FormatValue(originalVal)} -> {FormatValue(currentVal)}");
                            }
                        }
                        break;
                }
            }
            
            // Serialize
            if (oldValues.Count > 0) 
                auditEntry.OldValuesJson = System.Text.Json.JsonSerializer.Serialize(oldValues);
            
            if (newValues.Count > 0) 
                auditEntry.NewValuesJson = System.Text.Json.JsonSerializer.Serialize(newValues);

            // Summary
            var summary = string.Join("; ", changes);
            if (summary.Length > 2000) summary = summary.Substring(0, 1997) + "..."; // Increased limit or reliance on nvarchar(max)
            
            auditEntry.Summary = string.IsNullOrWhiteSpace(summary) ? $"{entry.State} {auditEntry.Entity}" : summary;

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private object MaskSensitiveData(string key, object value)
    {
        if (value == null) return null;
        
        var lowerKey = key.ToLower();
        if (lowerKey.Contains("password") || 
            lowerKey.Contains("cvv") || 
            lowerKey.Contains("token") || 
            lowerKey.Contains("secret") ||
            lowerKey.Contains("cardnumber")) // Partial mask for card?
        {
            return "***MASKED***";
        }
        
        return value;
    }

    private string FormatValue(object value)
    {
        if (value == null) return "null";
        // Simple heuristic to avoid logging massive blobs in summary if not needed, 
        // but for now standard toString is fine.
        return value.ToString();
    }

    private async Task OnAfterSaveChanges(List<AuditLog> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            // Logic to update IDs for added entities could go here if we tracked the temporary entries
            // For now, we accept that ID might be missing for AutoInc PKs on "Added" events in this simplified version
        }

        await this.AuditLogs.AddRangeAsync(auditEntries);
        await base.SaveChangesAsync(); 
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
