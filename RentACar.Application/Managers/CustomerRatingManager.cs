using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class CustomerRatingManager
    {
        private readonly ICustomerRatingRepository _ratingRepository;

        public CustomerRatingManager(ICustomerRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<CustomerRatingDisplayDto?> GetRatingByIdAsync(int ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);

            if (rating == null)
                return null;

            return MapToDisplayDto(rating);
        }

        public async Task<List<CustomerRatingDisplayDto>> GetRatingsByEmployeeIdAsync(int employeeId)
        {
            var ratings = await _ratingRepository.GetByEmployeeIdAsync(employeeId);

            return ratings.Select(MapToDisplayDto).ToList();
        }

        public async Task<List<CustomerRatingDisplayDto>> GetRatingsByUserIdAsync(int userId)
        {
            var ratings = await _ratingRepository.GetByCustomerIdAsync(userId);

            return ratings.Select(MapToDisplayDto).ToList();
        }

        public async Task<List<CustomerRatingDisplayDto>> GetAllRatingsAsync(string? searchTerm = null, string? sortColumn = null, string? sortDirection = null)
        {
            var ratings = await _ratingRepository.GetAllAsync(searchTerm, sortColumn, sortDirection);
            return ratings.Select(MapToDisplayDto).ToList();
        }

        public async Task<EmployeeRatingSummaryDto?> GetEmployeeRatingSummaryAsync(int employeeId)
        {
            var ratings = await _ratingRepository.GetByEmployeeIdAsync(employeeId);

            if (!ratings.Any())
                return null;

            return new EmployeeRatingSummaryDto
            {
                EmployeeId = employeeId,
                AverageStars = ratings.Average(r => r.Stars),
                TotalRatings = ratings.Count
            };
        }

        public async Task<int> AddRatingAsync(int userId, int bookingId, int stars, string? feedback)
        {
            // Validate star rating
            if (stars < 1 || stars > 5)
                throw new ArgumentException("Stars must be between 1 and 5.", nameof(stars));

            var rating = new CustomerRating
            {
                CustomerId = userId,  // CustomerId in entity maps to UserId in database
                BookingId = bookingId,
                Stars = stars,
                Feedback = feedback,
                RatingDate = DateTime.Now
            };

            await _ratingRepository.AddAsync(rating);

            return rating.RatingId;
        }

        public async Task<bool> DeleteRatingAsync(int ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);

            if (rating == null)
                return false;

            await _ratingRepository.DeleteAsync(ratingId);
            return true;
        }

        public Task<bool> UserCanRateEmployeeAsync(int userId, int employeeId)
        {
            // This method can be enhanced to check if user has completed bookings with this employee
            // For now, it returns true - you can add business logic as needed
            return Task.FromResult(true);
        }

        public async Task<RatingDetailsDto?> GetFullRatingDetailsAsync(int ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null) return null;

            return MapToDetailsDto(rating);
        }

        private CustomerRatingDisplayDto MapToDisplayDto(CustomerRating rating)
        {
            return new CustomerRatingDisplayDto
            {
                RatingId = rating.RatingId,
                BookingId = rating.BookingId,
                Stars = rating.Stars,
                Feedback = rating.Feedback,
                RatingDate = rating.RatingDate,
                CustomerId = rating.CustomerId,
                CustomerName = rating.Customer?.Name ?? "Unknown",
                CustomerEmail = rating.Customer?.User?.Email
            };
        }

        private RatingDetailsDto MapToDetailsDto(CustomerRating rating)
        {
            // Assuming Booking has related entities loaded
            var booking = rating.Booking;
            var payment = booking?.Payment;
            
            return new RatingDetailsDto
            {
                RatingId = rating.RatingId,
                BookingId = rating.BookingId,
                Stars = rating.Stars,
                Feedback = rating.Feedback,
                RatingDate = rating.RatingDate,
                CustomerId = rating.CustomerId,
                CustomerName = rating.Customer?.Name ?? "Unknown",
                CustomerEmail = rating.Customer?.User?.Email, 
                CustomerPhone = rating.Customer?.User?.PhoneNumber,
                BookingDate = booking != null ? booking.Startdate.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                EmployeeName = booking?.Employeebooker?.User?.UserName ?? "N/A",
                CarName = booking?.Car?.ModelName ?? "Unknown", 
                CarPlateNumber = booking?.Car?.PlateNumber ?? "Unknown",
                PaymentId = payment?.PaymentId,
                PaymentTotal = payment?.Amount,
                Promocode = booking?.Promocode?.Name
            };
        }
    }
}