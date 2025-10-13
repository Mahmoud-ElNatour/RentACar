# Booking.com Hotel & Flight Integration Plan

## 1. Goals
- Allow customers to search and reserve hotels and flights without leaving the RentACar portal.
- Reuse existing authentication, booking, and payment flows where possible.
- Maintain a clean separation between internal car-rental data and external travel data.

## 2. Booking.com API Overview
- **Hotel API**: Supports property search, rate retrieval, reservation creation, cancellation, and modification.
- **Flights API** (via Booking.com Flights or affiliated providers): Exposes flight search, fare rules, seat availability, and booking confirmation.
- Requires onboarding through Booking.com Partner Hub, sandbox credentials, and IP whitelisting.

## 3. Architecture Changes
### 3.1 Application Layer
- Add `IBookingComService` interface to abstract hotel and flight operations (`SearchHotelsAsync`, `SearchFlightsAsync`, `CreateReservationAsync`, `CancelReservationAsync`).
- Implement service in `RentACar.Infrastructure` using HttpClient and resilience policies (retry, circuit breaker via Polly).
- Introduce DTOs for hotel rooms, flight segments, and combined itineraries in `RentACar.Application`.

### 3.2 Web Layer
- Add new controllers:
  - `TravelController` for customer-facing pages (search forms, results, booking confirmation).
  - `TravelWebhookController` to receive asynchronous notifications (booking updates, cancellations).
- Create Razor views for hotel and flight search, results, itinerary review, and confirmation.
- Update layout navigation to add "Travel" entry for authenticated customers.

### 3.3 Infrastructure & Configuration
- Store API credentials in `appsettings.json` (with user secrets/KeyVault for production).
- Configure named `HttpClient` with base address, headers, and rate-limit handling.
- Persist external bookings in new tables:
  - `ExternalBookings` (BookingId, Type, Status, CustomerId, CreatedAt, Metadata JSON).
  - `ExternalBookingItems` (FK to ExternalBookings, ItemType, SupplierId, Price, Currency, Details JSON).
- Migrate database using EF Core migration.

## 4. User Journey
1. Customer opens Travel page, selects hotel or flight search.
2. Form submission calls `TravelController` which delegates to application manager using `IBookingComService`.
3. Results are normalized and displayed with sorting/filtering.
4. Customer selects option, system collects traveler/guest details.
5. Application posts reservation to Booking.com API.
6. On success, booking stored in ExternalBookings and optional payment captured via existing payment flow.
7. Confirmation page displays booking reference and itinerary.
8. Webhook updates keep reservation status in sync (e.g., cancellations).

## 5. Security & Compliance
- Use HTTPS and OAuth 2.0/API key as required by Booking.com.
- Obfuscate logs to avoid leaking PII or credential data.
- Implement rate limiting and request signing per API spec.
- Ensure GDPR compliance for storing traveler data; provide data removal workflows.

## 6. Error Handling & Resilience
- Wrap API calls with retries/backoff for transient errors.
- Define fallback messaging when API unavailable (e.g., "Travel services temporarily offline").
- Store request/response correlation IDs to aid support investigations.

## 7. Testing Strategy
- Unit tests for service layer using mocked HTTP responses.
- Integration tests with Booking.com sandbox covering hotel search and booking flows.
- UI/acceptance tests verifying multi-step booking process and webhook handling.
- Performance testing to ensure added endpoints do not degrade existing car booking flow.

## 8. Rollout Plan
- Phase 1: Hotel search & booking (beta, limited users).
- Phase 2: Flight search & booking, add bundling (car + hotel/flight packages).
- Phase 3: Cross-selling (suggest travel options during car checkout) and analytics dashboard updates.

## 9. Open Questions
- Confirm commercial agreement and commission structure with Booking.com.
- Determine if flights API will be direct or via third-party aggregator under Booking.com.
- Clarify payment responsibilities (prepaid through RentACar vs. pay-at-hotel/airline).

## 10. Next Steps
- Apply for Booking.com Partner API access and obtain sandbox credentials.
- Design detailed DTOs and database schema changes.
- Prototype hotel search flow with mocked data before live API integration.
- Review legal/privacy implications with compliance team.
