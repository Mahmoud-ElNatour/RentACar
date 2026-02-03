using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Core.Repositories;
using RentACar.Web.Models;
using System.Security.Claims;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverPortalController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDriverRepository _driverRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IDriverAvailabilityRepository _availabilityRepository;
        private readonly TripManager _tripManager;
        private readonly DriverManager _driverManager;
        private readonly IConfiguration _config;

        public DriverPortalController(
            UserManager<IdentityUser> userManager,
            IDriverRepository driverRepository,
            IBookingRepository bookingRepository,
            IDriverAvailabilityRepository availabilityRepository,
            TripManager tripManager,
            DriverManager driverManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _driverRepository = driverRepository;
            _bookingRepository = bookingRepository;
            _availabilityRepository = availabilityRepository;
            _tripManager = tripManager;
            _driverManager = driverManager;
            _config = config;
        }


        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
            if (driver == null) return Forbid();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var bookings = await _bookingRepository.GetBookingsByDriverIdAsync(driver.DriverId);

            var todayBookings = bookings
                .Where(b => b.Startdate <= today && b.Enddate >= today)
                .Select(b => new DriverPortalBookingViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.Customer?.Name ?? "N/A",
                    PickupLocationLabel = b.PickupLocationLabel ?? "N/A",
                    StartDate = b.Startdate,
                    EndDate = b.Enddate,
                    BookingStatus = b.BookingStatus ?? "Pending"
                }).ToList();

            var model = new DriverDashboardViewModel
            {
                DriverId = driver.DriverId,
                DriverName = driver.FullName,
                DriverCode = driver.DriverCode,
                IsAvailable = driver.IsActive,
                TodayBookings = todayBookings
            };

            ViewBag.GoogleMapsKey = _config["GOOGLE_MAPS_API_KEY"] ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");

            return View(model);
        }

        public async Task<IActionResult> Schedule()
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            if (driver == null) return Forbid();

            var bookings = await _bookingRepository.GetBookingsByDriverIdAsync(driver.DriverId);
            var availability = await _availabilityRepository.GetByDriverIdAsync(driver.DriverId);

            var model = new DriverScheduleViewModel
            {
                DriverId = driver.DriverId,
                DriverName = driver.FullName,
                MonthStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
                Bookings = bookings.Select(b => new DriverScheduleBookingItemViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.Customer?.Name ?? "N/A",
                    StartDate = b.Startdate,
                    EndDate = b.Enddate,
                    PickupLocationLabel = b.PickupLocationLabel ?? "N/A",
                    BookingStatus = b.BookingStatus ?? "Pending"
                }).ToList(),
                Availability = availability.Select(a => new DriverAvailabilityItemViewModel
                {
                    DriverAvailabilityId = a.DriverAvailabilityId,
                    Date = a.Date,
                    IsAvailable = a.IsAvailable,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    IsRecurringWeekly = a.IsRecurringWeekly
                }).ToList(),
                UpcomingBookings = bookings
                    .Where(b => b.Startdate >= DateOnly.FromDateTime(DateTime.Today))
                    .OrderBy(b => b.Startdate)
                    .Select(b => new DriverPortalBookingViewModel
                    {
                        BookingId = b.BookingId,
                        CustomerName = b.Customer?.Name ?? "N/A",
                        PickupLocationLabel = b.PickupLocationLabel ?? "N/A",
                        StartDate = b.Startdate,
                        EndDate = b.Enddate,
                        BookingStatus = b.BookingStatus ?? "Pending"
                    }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            if (driver == null) return Forbid();

            var booking = await _bookingRepository.GetBookingByIdAsync(id);
            if (booking == null || booking.DriverId != driver.DriverId) return NotFound();

            var trips = await _tripManager.GetTripsByDriverIdAsync(driver.DriverId);
            var currentTrip = trips.FirstOrDefault(t => t.BookingId == id);

            ViewBag.GoogleMapsKey = _config["GOOGLE_MAPS_API_KEY"] ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");

            var model = new DriverBookingDetailsViewModel
            {
                BookingId = booking.BookingId,
                BookingStatus = booking.BookingStatus ?? "Pending",
                TripStatus = currentTrip?.TripStatus ?? "Assigned",
                CarName = booking.Car?.ModelName ?? "N/A",
                CarPlate = booking.Car?.PlateNumber ?? "N/A",
                CustomerName = booking.Customer?.Name ?? "N/A",
                PickupLocationLabel = booking.PickupLocationLabel ?? "N/A",
                PickupLatitude = (double?)booking.PickupLatitude,
                PickupLongitude = (double?)booking.PickupLongitude,
                DriverLatitude = currentTrip != null ? (double?)currentTrip.LastDriverLatitude : null,
                DriverLongitude = currentTrip != null ? (double?)currentTrip.LastDriverLongitude : null,
                PickupDateTime = booking.PickupDateTime,
                DriverCode = driver.DriverCode,
                DriverId = driver.DriverId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTracking(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            var result = await _tripManager.StartTrackingAsync(bookingId, driver!.DriverId);
            if (!result.Success) TempData["TripError"] = result.Message;
            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkArrived(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            var result = await _tripManager.MarkArrivedAsync(bookingId, driver!.DriverId);
            if (!result.Success) TempData["TripError"] = result.Message;
            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrip(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            var result = await _tripManager.StartTripAsync(bookingId, driver!.DriverId);
            if (!result.Success) TempData["TripError"] = result.Message;
            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTrip(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            var result = await _tripManager.CompleteTripAsync(bookingId, driver!.DriverId);
            if (!result.Success) TempData["TripError"] = result.Message;
            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTrip(int bookingId, string reason)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            var result = await _tripManager.CancelTripAsync(bookingId, driver!.DriverId, reason);
            if (!result.Success) TempData["TripError"] = result.Message;
            return RedirectToAction(nameof(BookingDetails), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(DateOnly date, TimeOnly? startTime, TimeOnly? endTime, bool isAvailable = true)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);

            var availability = new Core.Entities.DriverAvailability
            {
                DriverId = driver!.DriverId,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                IsAvailable = isAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _availabilityRepository.AddAsync(availability);
            return RedirectToAction(nameof(Schedule));
        }

        public async Task<IActionResult> MapPanel(int? bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
            if (driver == null) return Forbid();

            DriverBookingDetailsViewModel? model = null;

            if (bookingId.HasValue)
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId.Value);
                if (booking != null && booking.DriverId == driver.DriverId)
                {
                    var trips = await _tripManager.GetTripsByDriverIdAsync(driver.DriverId);
                    var currentTrip = trips.FirstOrDefault(t => t.BookingId == bookingId.Value);

                    model = new DriverBookingDetailsViewModel
                    {
                        BookingId = booking.BookingId,
                        BookingStatus = booking.BookingStatus ?? "Pending",
                        TripStatus = currentTrip?.TripStatus ?? "Assigned",
                        CarName = booking.Car?.ModelName ?? "N/A",
                        CarPlate = booking.Car?.PlateNumber ?? "N/A",
                        CustomerName = booking.Customer?.Name ?? "N/A",
                        PickupLocationLabel = booking.PickupLocationLabel ?? "N/A",
                        PickupLatitude = (double?)booking.PickupLatitude,
                        PickupLongitude = (double?)booking.PickupLongitude,
                        DriverLatitude = currentTrip != null ? (double?)currentTrip.LastDriverLatitude : null,
                        DriverLongitude = currentTrip != null ? (double?)currentTrip.LastDriverLongitude : null,
                        PickupDateTime = booking.PickupDateTime,
                        DriverCode = driver.DriverCode,
                        DriverId = driver.DriverId
                    };
                }
            }

            return PartialView("_MapPanelPartial", model);
        }
    }
}
