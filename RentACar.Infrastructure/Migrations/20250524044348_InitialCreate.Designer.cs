﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RentACar.Infrastructure.Data;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    [DbContext(typeof(RentACarDbContext))]
    [Migration("20250524044348_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AspNetRoleAspNetUser", b =>
                {
                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("RoleId", "UserId");

                    b.ToTable("AspNetRoleAspNetUser");
                });

            modelBuilder.Entity("AspNetUserRole", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetRoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserToken", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("RentACar.Core.Entities.BlackList", b =>
                {
                    b.Property<int>("BlacklistId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("blacklistID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BlacklistId"));

                    b.Property<DateOnly>("DateBlocked")
                        .HasColumnType("date")
                        .HasColumnName("dateBlocked");

                    b.Property<int>("EmployeeDoneBlacklistId")
                        .HasColumnType("int")
                        .HasColumnName("employeeDoneBlacklistId");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("reason");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("userID");

                    b.HasKey("BlacklistId")
                        .HasName("PK_BlackList_1");

                    b.HasIndex("EmployeeDoneBlacklistId");

                    b.HasIndex("UserId");

                    b.ToTable("BlackList");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Booking", b =>
                {
                    b.Property<int>("BookingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("BookingID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BookingId"));

                    b.Property<string>("BookingStatus")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("bookingStatus");

                    b.Property<int>("CarId")
                        .HasColumnType("int")
                        .HasColumnName("carID");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int")
                        .HasColumnName("customerID");

                    b.Property<int?>("EmployeebookerId")
                        .HasColumnType("int")
                        .HasColumnName("EmployeebookerID");

                    b.Property<DateOnly>("Enddate")
                        .HasColumnType("date")
                        .HasColumnName("enddate");

                    b.Property<bool?>("IsBookedByEmployee")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false)
                        .HasColumnName("isBookedByEmployee");

                    b.Property<int>("PaymentId")
                        .HasColumnType("int")
                        .HasColumnName("paymentID");

                    b.Property<int?>("PromocodeId")
                        .HasColumnType("int")
                        .HasColumnName("promocodeID");

                    b.Property<DateOnly>("Startdate")
                        .HasColumnType("date")
                        .HasColumnName("startdate");

                    b.Property<decimal?>("Subtotal")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("subtotal");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("totalPrice");

                    b.HasKey("BookingId");

                    b.HasIndex("CarId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("EmployeebookerId");

                    b.HasIndex("PaymentId");

                    b.HasIndex("PromocodeId");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Car", b =>
                {
                    b.Property<int>("CarId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("CarID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CarId"));

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("categoryID");

                    b.Property<string>("Color")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("color");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit")
                        .HasColumnName("isAvailable");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("modelName");

                    b.Property<int>("ModelYear")
                        .HasColumnType("int")
                        .HasColumnName("modelYear");

                    b.Property<string>("PlateNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("plateNumber");

                    b.Property<decimal?>("PricePerDay")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("pricePerDay");

                    b.HasKey("CarId");

                    b.HasIndex("CategoryId");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("categoryID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("RentACar.Core.Entities.CreditCard", b =>
                {
                    b.Property<int>("CreditCardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("creditCardID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CreditCardId"));

                    b.Property<string>("CardHolderName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nchar(50)")
                        .HasColumnName("cardHolderName")
                        .IsFixedLength();

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("cardNumber");

                    b.Property<string>("Cvv")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nchar(10)")
                        .HasColumnName("cvv")
                        .IsFixedLength();

                    b.Property<DateOnly>("ExpiryDate")
                        .HasColumnType("date")
                        .HasColumnName("expiryDate");

                    b.HasKey("CreditCardId")
                        .HasName("PK_CreditCard_1");

                    b.ToTable("CreditCard");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Customer", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("userID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("DrivingLicenseBack")
                        .HasColumnType("image")
                        .HasColumnName("drivingLicenseBack");

                    b.Property<byte[]>("DrivingLicenseFront")
                        .HasColumnType("image")
                        .HasColumnName("drivingLicenseFront");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit")
                        .HasColumnName("isVerified");

                    b.Property<bool>("Isactive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("isactive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<byte[]>("NationalIdback")
                        .HasColumnType("image")
                        .HasColumnName("nationalIDBack");

                    b.Property<byte[]>("NationalIdfront")
                        .HasColumnType("image")
                        .HasColumnName("nationalIDFront");

                    b.Property<string>("aspNetUserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("aspNetUserId");

                    b.HasKey("UserId")
                        .HasName("PK_Customers_1");

                    b.HasIndex("aspNetUserId")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("RentACar.Core.Entities.CustomerCreditCard", b =>
                {
                    b.Property<int>("CustomerCreditCardId")
                        .HasColumnType("int");

                    b.Property<int>("CreditCardId")
                        .HasColumnType("int")
                        .HasColumnName("creditCardId");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("userId");

                    b.HasKey("CustomerCreditCardId");

                    b.HasIndex("CreditCardId");

                    b.HasIndex("UserId");

                    b.ToTable("CustomerCreditCard");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Employee", b =>
                {
                    b.Property<int>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("employeeID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EmployeeId"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("address");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit")
                        .HasColumnName("isActive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<decimal?>("Salary")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("salary");

                    b.Property<string>("aspNetUserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("aspNetUserId");

                    b.HasKey("EmployeeId");

                    b.HasIndex("aspNetUserId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("paymentID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PaymentId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("amount");

                    b.Property<int>("BookingId")
                        .HasColumnType("int")
                        .HasColumnName("bookingID");

                    b.Property<int?>("CreditcardId")
                        .HasColumnType("int")
                        .HasColumnName("creditcardID");

                    b.Property<DateOnly>("PaymentDate")
                        .HasColumnType("date")
                        .HasColumnName("paymentDate");

                    b.Property<string>("PaymentMethod")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)")
                        .HasColumnName("paymentMethod");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("status");

                    b.HasKey("PaymentId");

                    b.HasIndex("BookingId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Promocode", b =>
                {
                    b.Property<int>("PromocodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("promocodeID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PromocodeId"));

                    b.Property<string>("Description")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("DiscountPercentage")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("discountPercentage");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit")
                        .HasColumnName("isActive");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<DateOnly?>("ValidUntil")
                        .HasColumnType("date")
                        .HasColumnName("validUntil");

                    b.HasKey("PromocodeId");

                    b.ToTable("Promocodes");
                });

            modelBuilder.Entity("AspNetUserRole", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RentACar.Core.Entities.AspNetUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetRoleClaim", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetRole", "Role")
                        .WithMany("AspNetRoleClaims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserClaim", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithMany("AspNetUserClaims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserLogin", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithMany("AspNetUserLogins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUserToken", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithMany("AspNetUserTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.BlackList", b =>
                {
                    b.HasOne("RentACar.Core.Entities.Employee", "EmployeeDoneBlacklist")
                        .WithMany("BlackLists")
                        .HasForeignKey("EmployeeDoneBlacklistId")
                        .IsRequired()
                        .HasConstraintName("FK_BlackList_Employees");

                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithMany("BlackLists")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_BlackList_AspNetUsers1");

                    b.Navigation("EmployeeDoneBlacklist");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Booking", b =>
                {
                    b.HasOne("RentACar.Core.Entities.Car", "Car")
                        .WithMany("Bookings")
                        .HasForeignKey("CarId")
                        .IsRequired()
                        .HasConstraintName("FK_Bookings_Cars1");

                    b.HasOne("RentACar.Core.Entities.Customer", "Customer")
                        .WithMany("Bookings")
                        .HasForeignKey("CustomerId")
                        .IsRequired()
                        .HasConstraintName("FK_Bookings_Customers1");

                    b.HasOne("RentACar.Core.Entities.Employee", "Employeebooker")
                        .WithMany("Bookings")
                        .HasForeignKey("EmployeebookerId")
                        .HasConstraintName("FK_Bookings_Employees");

                    b.HasOne("RentACar.Core.Entities.Payment", "Payment")
                        .WithMany("Bookings")
                        .HasForeignKey("PaymentId")
                        .IsRequired()
                        .HasConstraintName("FK_Bookings_Payments");

                    b.HasOne("RentACar.Core.Entities.Promocode", "Promocode")
                        .WithMany("Bookings")
                        .HasForeignKey("PromocodeId")
                        .HasConstraintName("FK_Bookings_Promocodes1");

                    b.Navigation("Car");

                    b.Navigation("Customer");

                    b.Navigation("Employeebooker");

                    b.Navigation("Payment");

                    b.Navigation("Promocode");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Car", b =>
                {
                    b.HasOne("RentACar.Core.Entities.Category", "Category")
                        .WithMany("Cars")
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_Cars_Categories1");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Customer", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithOne("Customer")
                        .HasForeignKey("RentACar.Core.Entities.Customer", "aspNetUserId")
                        .IsRequired()
                        .HasConstraintName("FK_Customers_AspNetUsers");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.CustomerCreditCard", b =>
                {
                    b.HasOne("RentACar.Core.Entities.CreditCard", "CreditCard")
                        .WithMany("CustomerCreditCards")
                        .HasForeignKey("CreditCardId")
                        .IsRequired()
                        .HasConstraintName("FK_CustomerCreditCard_CreditCard");

                    b.HasOne("RentACar.Core.Entities.Customer", "User")
                        .WithMany("CustomerCreditCards")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_CustomerCreditCard_Customers");

                    b.Navigation("CreditCard");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Employee", b =>
                {
                    b.HasOne("RentACar.Core.Entities.AspNetUser", "User")
                        .WithMany("Employees")
                        .HasForeignKey("aspNetUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Payment", b =>
                {
                    b.HasOne("RentACar.Core.Entities.Booking", "Booking")
                        .WithMany("Payments")
                        .HasForeignKey("BookingId")
                        .IsRequired()
                        .HasConstraintName("FK_Payments_Bookings");

                    b.Navigation("Booking");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetRole", b =>
                {
                    b.Navigation("AspNetRoleClaims");
                });

            modelBuilder.Entity("RentACar.Core.Entities.AspNetUser", b =>
                {
                    b.Navigation("AspNetUserClaims");

                    b.Navigation("AspNetUserLogins");

                    b.Navigation("AspNetUserTokens");

                    b.Navigation("BlackLists");

                    b.Navigation("Customer");

                    b.Navigation("Employees");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Booking", b =>
                {
                    b.Navigation("Payments");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Car", b =>
                {
                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Category", b =>
                {
                    b.Navigation("Cars");
                });

            modelBuilder.Entity("RentACar.Core.Entities.CreditCard", b =>
                {
                    b.Navigation("CustomerCreditCards");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Customer", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("CustomerCreditCards");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Employee", b =>
                {
                    b.Navigation("BlackLists");

                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Payment", b =>
                {
                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("RentACar.Core.Entities.Promocode", b =>
                {
                    b.Navigation("Bookings");
                });
#pragma warning restore 612, 618
        }
    }
}
