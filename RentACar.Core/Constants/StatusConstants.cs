namespace RentACar.Core.Constants
{
    public static class BookingStatus
    {
        public const string Pending = "Pending";// until paymentdone successfully
        public const string Confirmed = "Confirmed";// paid but waiting customer to come sign contract and takes the car
        public const string InProgress = "InProgress";// customer takes the car
        public const string AwaitingReturn = "AwaitingReturn";// booking is overdue, waiting customer to return the car
        public const string Completed = "Completed";// customer has returned the car - end of booking cycle
        public const string Cancelled = "Cancelled";// booking is cancelled
        public const string Rejected = "Rejected"; // Keeping for admin rejection flow
    }

    public static class PaymentStatus
    {
        public const string Pending = "Pending";// default until paymentdone successfully
        public const string Paid = "Paid";// paid successfully
        public const string Cancelled = "Cancelled";// payment is cancelled (booking canclled - waitong to refund the payment to the customer)
        public const string Refunded = "Refunded";// payment is refunded (not necessary booking cancelation)
        public const string Failed = "Failed";// payment failed
    }
}
