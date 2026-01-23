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
        public int PaymentReminderDelayHours { get; set; } = 24;
        public bool PaymentReminderSendOnceOnly { get; set; } = true;
        public int? PaymentReminderRepeatEveryHours { get; set; }
        public int? PaymentReminderMaxSends { get; set; }
        public string PaymentReminderStatusCsv { get; set; } = "Pending";

        // --- Pickup Reminder Config ---
        public bool PickupReminderEnabled { get; set; } = true;
        public int PickupReminderHoursBefore { get; set; } = 24;
        public bool PickupReminderSendOnceOnly { get; set; } = true;

        // --- Return Reminder Config ---
        public bool ReturnReminderEnabled { get; set; } = true;
        public int ReturnReminderHoursBefore { get; set; } = 24;
        public bool ReturnReminderSendOnceOnly { get; set; } = true;

        // --- Promo Expiry Config ---
        public bool PromoExpiryEnabled { get; set; } = true;
        public string PromoExpiryCheckFrequency { get; set; } = "Daily";
        public bool PromoExpiryAutoDeactivate { get; set; } = true;

        // --- Distribution List Mappings (Internal Groups) ---
        public int? PromoExpiryEmployeesListId { get; set; }
        public int? CarUpdateEmployeesListId { get; set; }
        public int? CategoryUpdateEmployeesListId { get; set; }
        public int? PromocodeUpdateEmployeesListId { get; set; }
        public int? DocsReminderEmployeesListId { get; set; }
        public int? EmployeesDefaultListId { get; set; } // Fallback

        // --- Metadata ---
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        
        [ForeignKey("UpdatedByUserId")]
        public virtual AspNetUser? UpdatedByUser { get; set; }
    }
}
