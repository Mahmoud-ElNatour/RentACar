$path = "c:\Users\Mohammad\Downloads\Mahmoud\RentACar\RentACar.Application\Managers\BookingManager.cs"
$content = Get-Content $path -Raw

# Car Validation
$carConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*if \(car == null\).*?>>>>>>> Mahmoud-V3\r?\n"
$carFixed = @'
            if (car == null)
            {
                _logger.LogWarning("Booking failed: Car not found.");
                return new BookingCreationResultDto { Success = false, ErrorMessage = "Car not found." };
            }
            if (!car.IsAvailable)
            {
                _logger.LogWarning("Booking failed: Car is not available.");
                return new BookingCreationResultDto { Success = false, ErrorMessage = "This car is currently not available for booking." };
            }
'@
$content = [regex]::Replace($content, $carConflict, $carFixed)

# Audit/Email
$auditConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*_logger\.LogInformation\(""✅ Booking created.*?>>>>>>> Mahmoud-V3\r?\n"
$auditFixed = @'
            _logger.LogInformation("✅ Booking created with ID: {BookingId} Status: {Status}", addedBooking.BookingId, addedBooking.BookingStatus);
            
            await _auditLogManager.LogEventAsync("Booking.Created", "Booking", addedBooking.BookingId.ToString(), $"Created new booking for Car {addedBooking.CarId}. Status: {addedBooking.BookingStatus}", null, "Success");

            if (addedBooking.HasDriver && addedBooking.DriverId.HasValue)
            {
                await _auditLogManager.LogEventAsync("Booking.DriverAssigned", "Booking", addedBooking.BookingId.ToString(), $"Driver {addedBooking.DriverId} assigned to booking.", null, "Success");
            }
            
            // 📨 Send Booking Status Email (Pending)
            if (isCustomer) 
            {
                 var cust = await _customerRepository.GetByIdAsync(addedBooking.CustomerId);
                 var custUser = await _userManager.FindByIdAsync(cust.aspNetUserId);
                 if (custUser != null)
                 {
                     await _emailManager.SendBookingStatusEmail(custUser.Email, cust.Name, addedBooking);
                 }
            }
            
            string? checkoutUrl = null;
            if (!isCash)
            {
                var session = await _paymentManager.CreateCheckoutSessionForPaymentAsync(addedPayment);
                if (string.IsNullOrWhiteSpace(session.CheckoutUrl))
                {
                    _logger.LogWarning("Stripe checkout session missing URL for booking {BookingId}", addedBooking.BookingId);
                }
                checkoutUrl = session.CheckoutUrl;
            }
'@
$content = [regex]::Replace($content, $auditConflict, $auditFixed)

# Blocking Status
$blockingConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*return !status\.Equals\(BookingStatus\.Completed.*?>>>>>>> Mahmoud-V3\r?\n"
$blockingFixed = @'
            return !status.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase)
                && !status.Equals("Returned", StringComparison.OrdinalIgnoreCase) // Legacy status support
                && !status.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase)
                && !status.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
'@
$content = [regex]::Replace($content, $blockingConflict, $blockingFixed)

Set-Content $path -Value $content -NoNewline
Write-Host "Done"
