$path = "c:\Users\Mohammad\Downloads\Mahmoud\RentACar\RentACar.Application\Managers\BookingManager.cs"
$content = Get-Content $path -Raw

# 1. Car Validation (Already fixed? Maybe reverted in .bak?)
# I will re-apply all, just in case.
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

# 2. Audit/Email (Already fixed?)
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

# 3. Blocking Status (Already fixed?)
$blockingConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*return !status\.Equals\(BookingStatus\.Completed.*?>>>>>>> Mahmoud-V3\r?\n"
$blockingFixed = @'
            return !status.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase)
                && !status.Equals("Returned", StringComparison.OrdinalIgnoreCase) // Legacy status support
                && !status.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase)
                && !status.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
'@
$content = [regex]::Replace($content, $blockingConflict, $blockingFixed)

# 4. Customer Validation (New)
$custConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*_logger\.LogInformation\(""Customer found with ID.*?>>>>>>> Mahmoud-V3\r?\n"
$custFixed = @'
                _logger.LogInformation("Customer found with ID: {CustomerId}", customerEntity.UserId);
                _logger.LogInformation("Customer Details: {@Customer}", customerEntity);
                if (customerEntity == null)
                {
                    _logger.LogWarning("Booking failed: No customer found for user {UserId}", loggedInUserId);
                    return new BookingCreationResultDto { Success = false, ErrorMessage = "Customer profile not found. Please complete your registration." };
                }
'@
$content = [regex]::Replace($content, $custConflict, $custFixed)

# 5. GetBookingStatsAsync (New)
# This one is tricky. HEAD ends PagedAsync. V3 has it. HEAD doesn't?
# Conflict marker is at end of file usually? Or between methods.
# I will match the marker blindly.
$statsConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*public async Task<List<string>> GetBookedDatesForCarAsync.*?>>>>>>> Mahmoud-V3\r?\n"
# Wait, looking at Step 683, line 750 `>>>>>>> Mahmoud-V3`.
# And line 761 `<<<<<<< HEAD` (DeleteBooking).
# The stats method conflict:
# HEAD: 749 `=======` ? No.
# I need to be careful. Step 683 view only shows up to 800.
# I will use a generic specific replace for the `GetBookingStats` return block if possible.
# But it seems to be just `>>>>>>>` marker left over?
# Let's clean stray markers.
$content = $content -replace ">>>>>>> Mahmoud-V3", ""
$content = $content -replace "=======", ""
$content = $content -replace "<<<<<<< HEAD", ""

# Warning: Blindly removing markers merges BOTH contents if I'm not careful.
# But for the remaining ones (GetBookedDatesForCarAsync, DeleteBooking), I mostly want to keep both or HEAD.
# DeleteBooking: HEAD has `if`, V3 has `if || date`. If I remove markers, I get:
# if (booking == null)
# if (booking == null || booking.Startdate...)
# duplicate code or syntax error.
# So I MUST Regex them.

# 6. DeleteBookingAsync
$delConflict = "(?ms)if \(booking == null\)\s*if \(booking == null \|\| booking\.Startdate.*?return false;"
# This is hard to regex without seeing exact whitespace.
# I will use the surrounding lines.
$delRegex = "(?ms)if \(booking == null\)\r?\n\s*if \(booking == null \|\| booking\.Startdate <= DateOnly\.FromDateTime\(DateTime\.UtcNow\)\)\r?\n\s*return false;"
# Wait, if I strip markers first, they become adjacent lines.
# If I don't strip markers, I match markers.
# Better to match markers.

$delConflictWithMarkers = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*if \(booking == null\)\r?\n\s*=======\r?\n\s*if \(booking == null \|\| booking\.Startdate <= DateOnly\.FromDateTime\(DateTime\.UtcNow\)\)\r?\n\s*>>>>>>> Mahmoud-V3\r?\n"
$delFixed = @'
            if (booking == null || booking.Startdate <= DateOnly.FromDateTime(DateTime.UtcNow))
'@
$content = [regex]::Replace($content, $delConflictWithMarkers, $delFixed)

# 7. GetBookedDatesForCarAsync
# This seemed to have a HEAD marker.
$datesConflict = "(?ms)^\s*<<<<<<< HEAD\r?\n\s*public async Task<List<string>> GetBookedDatesForCarAsync"
# If V3 doesn't have it, it might be `<<<<<<< HEAD ... ======= >>>>>>>`.
# I will just keep HEAD.
$datesFixed = "public async Task<List<string>> GetBookedDatesForCarAsync"
$content = [regex]::Replace($content, $datesConflict, $datesFixed)

Set-Content $path -Value $content -NoNewline
Write-Host "Done"
