using System;
using System.Collections.Generic;

namespace RentACar.Application.DTOs.Support
{
    public class AiSupportContext
    {
        public AiUserContext UserContext { get; set; } = new();
        public AiGlobalContext GlobalContext { get; set; } = new();
        
        // Backward compatibility (optional, can be removed if not used)
        public string CustomerName => UserContext.Name;
        public bool IsVerified => UserContext.IsVerified;
        public AiBookingInfo? ActiveBooking => UserContext.ActiveBooking;
    }

    public class AiUserContext
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = "Guest";
        public bool IsVerified { get; set; }
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = ""; // Added Phone
        
        public AiBookingInfo? ActiveBooking { get; set; }
        public List<AiBookingInfo> RecentBookings { get; set; } = new();
        public List<AiPaymentInfo> RecentPayments { get; set; } = new();
        
        // Latest Trip info for active booking
        public AiTripInfo? CurrentTrip { get; set; }
    }

    public class AiGlobalContext
    {
        public AiCompanyInfo Company { get; set; } = new(); // Added Company Info
        public List<string> InventorySummary { get; set; } = new();
        public List<string> AllCategories { get; set; } = new(); // Added Categories List
        public List<string> ActivePromotions { get; set; } = new();
        public List<string> PaymentMethods { get; set; } = new();
        
        // New: Detailed real-time availability and definitions
        public List<string> FleetAvailability { get; set; } = new();
        public Dictionary<string, string> StatusDefinitions { get; set; } = new();

        public AiPolicyInfo Policies { get; set; } = new();
    }

    public class AiCompanyInfo
    {
        public string Email { get; set; } = "support@rentacar.com"; // Default, will overwrite in Manager
        public string PhoneNumber { get; set; } = "+961 1 234 567";
        public string Address { get; set; } = "Beirut, Lebanon";
    }

    public class AiPolicyInfo
    {
        public string CancellationPolicy { get; set; } = "Free cancellation up to 48 hours before pickup. Late cancellations incur a 1-day fee.";
        public string DriverRequirements { get; set; } = "Minimum age 21. Valid driver's license held for at least 1 year. International Permit required for non-residents.";
        public string SecurityDeposit { get; set; } = "Security deposit ranges from $300-$1000 depending on car category, refundable within 5-7 business days.";
        public string VerificationSteps { get; set; } = "1. Upload ID (Front/Back). 2. Upload Driver's License. 3. Selfie verification. Approval typically takes 1-2 hours.";
    }

    public class AiBookingInfo
    {
        public int BookingId { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Car Details
        public string CarName { get; set; }
        public string PlateNumber { get; set; }
        public string Color { get; set; }
        public string Category { get; set; }
        
        // Pickup Details
        public string PickupAddress { get; set; }
        public string PickupLocationLabel { get; set; }
        public DateTime PickupDateTime { get; set; }
        
        // Driver
        public bool HasDriver { get; set; }
        public decimal DriverFee { get; set; }
    }

    public class AiPaymentInfo
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Method { get; set; }
    }

    public class AiTripInfo
    {
        public string Status { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; } // Only share if active
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? EstimatedArrival { get; set; }
    }
}
