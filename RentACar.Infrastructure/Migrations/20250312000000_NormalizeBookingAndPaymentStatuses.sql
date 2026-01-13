-- Normalize booking statuses to: Booked, Pending, Rejected, Returned
UPDATE Bookings
SET bookingStatus = CASE
    WHEN bookingStatus IS NULL THEN bookingStatus
    WHEN LOWER(bookingStatus) = 'accepted' THEN 'Booked'
    WHEN LOWER(bookingStatus) = 'booked' THEN 'Booked'
    WHEN LOWER(bookingStatus) = 'pending' THEN 'Pending'
    WHEN LOWER(bookingStatus) = 'returned' THEN 'Returned'
    WHEN LOWER(bookingStatus) = 'rejected' THEN 'Rejected'
    WHEN LOWER(bookingStatus) = 'cancelled' THEN 'Rejected'
    WHEN LOWER(bookingStatus) = 'canceled' THEN 'Rejected'
    ELSE bookingStatus
END;

-- Normalize payment statuses to: Paid, Unpaid, Cancelled
UPDATE Payments
SET status = CASE
    WHEN status IS NULL THEN status
    WHEN LOWER(status) IN ('done', 'paid') THEN 'Paid'
    WHEN LOWER(status) IN ('pending', 'unpaid') THEN 'Unpaid'
    WHEN LOWER(status) IN ('cancelled', 'canceled', 'rejected') THEN 'Cancelled'
    ELSE status
END;
