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
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Driver> Drivers { get; set; }
    public virtual DbSet<DriverAvailability> DriverAvailabilities { get; set; }
    public virtual DbSet<DriverLocationPing> DriverLocationPings { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
    public virtual DbSet<Promocode> Promocodes { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<CustomerRating> CustomerRatings { get; set; }
    public virtual DbSet<DistributionList> DistributionLists { get; set; }
    public virtual DbSet<DistributionListMember> DistributionListMembers { get; set; }
    public virtual DbSet<DistributionListRule> DistributionListRules { get; set; }
    public virtual DbSet<EmailDraft> EmailDrafts { get; set; }
    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
    public virtual DbSet<NotificationSettings> NotificationSettings { get; set; }
    public virtual DbSet<EmailLog> EmailLogs { get; set; }
    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }
    public virtual DbSet<SenderIdentity> SenderIdentities { get; set; }
    public virtual DbSet<EmailFeatureConfig> EmailFeatureConfigs { get; set; }
    public virtual DbSet<SupportConversation> SupportConversations { get; set; }
    public virtual DbSet<SupportMessage> SupportMessages { get; set; }
    public virtual DbSet<AiConversation> AiConversations { get; set; }
    public virtual DbSet<AiMessage> AiMessages { get; set; }





    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Email / Distribution Lists
        modelBuilder.Entity<DistributionList>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<DistributionListMember>(entity =>
        {
            entity.HasIndex(e => new { e.DistributionListId, e.Email }).IsUnique();
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasIndex(e => e.TemplateKey).IsUnique();
        });

        // Foreign Key Configurations for User Content
        modelBuilder.Entity<EmailDraft>()
            .HasOne(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DistributionList>()
            .HasOne(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<DistributionList>()
            .HasOne(d => d.UpdatedByUser)
            .WithMany()
            .HasForeignKey(d => d.UpdatedByUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<DistributionListMember>()
            .HasOne(d => d.AddedByUser)
            .WithMany()
            .HasForeignKey(d => d.AddedByUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<EmailTemplate>()
            .HasOne(d => d.UpdatedByUser)
            .WithMany()
            .HasForeignKey(d => d.UpdatedByUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Logs - Preserve History
        modelBuilder.Entity<EmailLog>()
            .HasOne(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<AuditLog>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

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

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(d => d.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Payments_Bookings");

            // Indexes for Performance
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Payments_Status");
            entity.HasIndex(e => e.PaymentDate).HasDatabaseName("IX_Payments_PaymentDate");
            entity.HasIndex(e => e.BookingId).HasDatabaseName("IX_Payments_BookingId");
            entity.HasIndex(e => new { e.Status, e.PaymentDate }).HasDatabaseName("IX_Payments_Status_PaymentDate");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Customers_1");
            entity.Property(e => e.Isactive).HasDefaultValue(true);
            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customers_AspNetUsers");

            // Indexes for Performance
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_Customers_Name");
            entity.HasIndex(e => e.aspNetUserId).HasDatabaseName("IX_Customers_AspNetUserId");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.IsBookedByEmployee).HasDefaultValue(false);
            entity.Property(e => e.HasDriver).HasDefaultValue(false);

            entity.HasOne(d => d.Car).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Cars1");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Customers1");

            entity.HasOne(d => d.Employeebooker).WithMany(p => p.Bookings)
                .HasConstraintName("FK_Bookings_Employees");

            entity.HasOne(d => d.Driver).WithMany(p => p.Bookings)
                .HasConstraintName("FK_Bookings_Drivers");

            entity.HasOne(d => d.Promocode).WithMany(p => p.Bookings)
               .HasConstraintName("FK_Bookings_Promocodes1");

            // Indexes for Performance
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Bookings_CustomerId");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.Cars)
                .HasConstraintName("FK_Cars_Categories1");
        });

        // Driver Entitites Configuration


        modelBuilder.Entity<Driver>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.Drivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Drivers_AspNetUsers");

            entity.HasOne(d => d.Employee).WithOne(p => p.Driver)
                .HasForeignKey<Driver>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Drivers_Employees");

            entity.HasIndex(d => d.EmployeeId).IsUnique();
        });

        modelBuilder.Entity<DriverAvailability>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.Date }).IsUnique();

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverAvailabilities)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverAvailability_Drivers");
        });

        modelBuilder.Entity<DriverLocationPing>(entity =>
        {
            entity.HasOne(d => d.Booking).WithMany(p => p.DriverLocationPings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverLocationPings_Bookings");

            entity.HasOne(d => d.Driver).WithMany(p => p.LocationPings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverLocationPings_Drivers");
        });

        // Trip config
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasIndex(e => e.BookingId).IsUnique();

            entity.Property(e => e.TripStatus)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasOne(d => d.Booking)
                .WithOne(p => p.Trip)
                .HasForeignKey<Trip>(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Trips_Bookings");

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.Trips)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK_Trips_Drivers");
        });

        modelBuilder.Entity<SupportConversation>(entity =>
        {
            entity.HasOne(d => d.Customer)
                .WithMany()
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.AssignedEmployee)
                .WithMany()
                .HasForeignKey(d => d.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_SupportConversations_CustomerId");
            entity.HasIndex(e => e.BookingId).HasDatabaseName("IX_SupportConversations_BookingId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_SupportConversations_Status");
            entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("IX_SupportConversations_UpdatedAt");
        });

        modelBuilder.Entity<SupportMessage>(entity =>
        {
            entity.HasOne(d => d.Conversation)
                .WithMany(p => p.Messages)
                .HasForeignKey(d => d.SupportConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Sender)
                .WithMany()
                .HasForeignKey(d => d.SenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AiConversation>(entity =>
        {
            entity.HasOne(d => d.Customer)
                .WithMany()
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_AiConversations_CustomerId");
            entity.HasIndex(e => e.LastActiveAt).HasDatabaseName("IX_AiConversations_LastActiveAt");
        });

        modelBuilder.Entity<AiMessage>(entity =>
        {
            entity.HasOne(d => d.Conversation)
                .WithMany(p => p.Messages)
                .HasForeignKey(d => d.AiConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Sender)
                .HasConversion<string>(); // Store Enum as String for readability
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

