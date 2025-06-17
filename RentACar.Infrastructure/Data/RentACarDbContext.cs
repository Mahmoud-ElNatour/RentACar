using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;

namespace RentACar.Infrastructure.Data;

public partial class RentACarDbContext : DbContext
{
    public RentACarDbContext(DbContextOptions<RentACarDbContext> options)
        : base(options)
    {
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

            entity.HasOne(d => d.Employeebooker).WithMany(p => p.Bookings).HasConstraintName("FK_Bookings_Employees");

            entity.HasOne(d => d.Payment).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Payments");

            entity.HasOne(d => d.Promocode).WithMany(p => p.Bookings).HasConstraintName("FK_Bookings_Promocodes1");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.Cars).HasConstraintName("FK_Cars_Categories1");
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
            entity.HasKey(e => new { e.UserId, e.CreditCardId }); // Composite PK

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
            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Bookings");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
