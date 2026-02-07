using System;

namespace RentACar.Application.Services
{
    public sealed class BookingPricingBreakdown
    {
        public int RentalDays { get; init; }
        public decimal BaseRental { get; init; }
        public decimal DriverService { get; init; }
        public decimal CarExtraDriverFee { get; init; }
        public decimal Subtotal { get; init; }
        public decimal Discount { get; init; }
        public decimal Total { get; init; }
    }

    public static class BookingPricingCalculator
    {
        public static BookingPricingBreakdown Calculate(
            decimal pricePerDay,
            DateOnly startDate,
            DateOnly endDate,
            bool hasDriver,
            decimal? driverDailyFee,
            decimal? extraDriverFeePerDay,
            decimal? promoDiscountPercentage)
        {
            var rentalDays = GetRentalDays(startDate, endDate);
            var baseRental = pricePerDay * rentalDays;
            var driverService = hasDriver && driverDailyFee.HasValue ? driverDailyFee.Value * rentalDays : 0m;
            var carExtra = hasDriver && extraDriverFeePerDay.HasValue ? extraDriverFeePerDay.Value * rentalDays : 0m;
            var subtotal = baseRental + driverService + carExtra;
            var discount = promoDiscountPercentage.HasValue ? subtotal * (promoDiscountPercentage.Value / 100m) : 0m;
            var total = subtotal - discount;

            return new BookingPricingBreakdown
            {
                RentalDays = rentalDays,
                BaseRental = baseRental,
                DriverService = driverService,
                CarExtraDriverFee = carExtra,
                Subtotal = subtotal,
                Discount = discount,
                Total = total
            };
        }

        public static int GetRentalDays(DateOnly startDate, DateOnly endDate)
        {
            var duration = endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
            return duration.Days + 1;
        }
    }
}
