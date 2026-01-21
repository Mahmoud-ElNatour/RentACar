using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class NotificationSettings
    {
        [Key]
        public int Id { get; set; } // Always 1

        // Master Switches
        public bool ReminderProcessingEnabled { get; set; }
        public bool AllNotificationEmailsEnabled { get; set; }
        public int CheckIntervalMinutes { get; set; } = 60;

        // Payment Reminder
        public bool PaymentReminderEnabled { get; set; }
        public int PaymentReminderDelayHours { get; set; } = 24;
        public int? PaymentReminderRepeatEveryHours { get; set; }
        public int? PaymentReminderMaxSends { get; set; }

        // Pickup Reminder
        public bool PickupReminderEnabled { get; set; }
        public int PickupReminderHoursBefore { get; set; } = 24;

        // Return Reminder
        public bool ReturnReminderEnabled { get; set; }
        public int ReturnReminderHoursBefore { get; set; } = 24;

        // Promo Expiry
        public bool PromoExpiryEnabled { get; set; }
        public bool PromoExpiryAutoDeactivate { get; set; } = true;
        // "Hourly", "Daily"
        public string PromoExpiryCheckFrequency { get; set; } = "Daily";

        // Linked Distribution Lists for Internal Notifications
        public int? EmployeesDefaultListId { get; set; }
        public int? PromoExpiryEmployeesListId { get; set; }
        public int? CarUpdateEmployeesListId { get; set; }
        public int? CategoryUpdateEmployeesListId { get; set; }
        public int? PromocodeUpdateEmployeesListId { get; set; }
    }
}
