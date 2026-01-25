using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class NotificationSettings
    {
        [Key]
        public int Id { get; set; } // Always 1

        // --- Processing State ---
        public bool ReminderProcessingEnabled { get; set; } = true;
        public bool ReminderProcessingPaused { get; set; } = false;
        public bool AllNotificationEmailsEnabled { get; set; } = true;
        public int CheckIntervalMinutes { get; set; } = 60;

        // --- Last Run Statistics (for Dashboard Header) ---
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public int LastRunProcessedCount { get; set; } = 0;
        public int LastRunSentCount { get; set; } = 0;
        public int LastRunFailedCount { get; set; } = 0;
        public string? LastRunSummary { get; set; }

        // --- Payment Reminder Config ---
        public bool PaymentReminderEnabled { get; set; } = true;
        public int PaymentReminderInitialDelayHours { get; set; } = 6; // Start checking after 6 hours
        public int PaymentReminderIntervalHours { get; set; } = 6; // Repeat every 6 hours
        public int PaymentReminderMaxDurationHours { get; set; } = 48; // Stop after 48 hours
        public string? PaymentReminderStatusCsv { get; set; } = "Pending";

        // --- Pickup Reminder Config ---
        public bool PickupReminderEnabled { get; set; } = true;
        public string? PickupReminderScheduleHoursCsv { get; set; } = "24,1"; // Alerts at 24h before and 1h before

        // --- Return Reminder Config ---
        public bool ReturnReminderEnabled { get; set; } = true;
        public string? ReturnReminderScheduleHoursCsv { get; set; } = "24,2"; // Alerts at 24h before and 2h before

        // --- Promo Expiry Config ---
        public bool PromoExpiryEnabled { get; set; } = true;
        public int PromoExpiryDaysBefore { get; set; } = 1; // Notify 1 day before
        public string? PromoExpiryCheckFrequency { get; set; } = "Daily";
        public bool PromoExpiryAutoDeactivate { get; set; } = true;



        // --- Metadata ---
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        
        [ForeignKey("UpdatedByUserId")]
        public virtual AspNetUser? UpdatedByUser { get; set; }
    }
}
