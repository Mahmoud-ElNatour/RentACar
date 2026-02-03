namespace RentACar.Core.Constants
{
    public static class SupportCategory
    {
        public const string Booking = "Booking";
        public const string Payment = "Payment";
        public const string Account = "Account";
        public const string Complaint = "Complaint";
        public const string Other = "Other";

        public static readonly string[] All = { Booking, Payment, Account, Complaint, Other };
    }

    public static class SupportStatus
    {
        public const string Open = "Open";
        public const string Assigned = "Assigned";
        public const string Resolved = "Resolved";
        public const string Closed = "Closed";

        public static readonly string[] All = { Open, Assigned, Resolved, Closed };
    }

    public static class SenderRole
    {
        public const string Customer = "Customer";
        public const string Employee = "Employee";
        public const string System = "System";
    }
}
