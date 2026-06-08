# COMMUNITY EVENT MANAGEMENT SYSTEM (CEMS)

## SYSTEM REQUIREMENTS SPECIFICATION (SRS) v1.0

**Document Status:** ✅ FINALISED &nbsp;|&nbsp; **Release:** v1.0 &nbsp;|&nbsp; **Last Updated:** June 8, 2026
**Module:** CET254 Advanced Programming — Assignment 1 &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
**Classification:** Academic Submission — University of Sunderland

---

## EXECUTIVE SUMMARY

This System Requirements Specification (SRS) documents the functional requirements (FRs),
non-functional requirements (NFRs) and technical specification of the Community Event Management
System (CEMS). It is the developer- and tester-facing companion to the Business Requirements
Specification (`CEMS_BRS_v1.0.md`): the BRS says *what the business needs*, this SRS says *what the
system does and how it is built*.

**Target Users**

- **Administrator** — back-office staff who manage events, venues, activities and participants.
- **Registered User** — a member of the public with a self-service account who browses and registers
  for events and manages their own bookings.
- **Visitor** — an unauthenticated user who can browse events and create an account.

**Platforms**

- Server-rendered web application — **.NET 10 Blazor (Interactive Server)**.
- Responsive design (desktop primary, mobile supported).
- Modern evergreen browsers (Chrome, Edge, Firefox — latest 2 versions).

> **Note on the "Service & Method" sections.** CEMS is a Blazor Server application, so its use cases
> are exposed through **C# service interfaces** consumed directly by the components (there is no
> public REST API). Where a traditional SRS would list API endpoints, this document lists the
> equivalent **service contract methods**.

---

## FUNCTIONAL REQUIREMENTS (FRs)

### FR-001: User Authentication & Authorisation

**Requirement:** The system shall authenticate users and enforce role-based access.

**Details:**

1. Login by e-mail + password; the password is verified against a **BCrypt** hash.
2. A successful login issues a cookie (8-hour expiry); logout clears it.
3. Three roles — Administrator, Registered User, Visitor — gate navigation and pages.
4. The login form posts to an MVC `AuthController` so the cookie is written during a real HTTP
   request (Blazor SSR cannot set cookies mid-render).

**Service & Methods:**

```
AuthController (MVC):  POST /auth/login   POST /auth/logout   GET /auth/test
IAuthService:          Task<bool> LoginAsync(string email, string password)
                       Task LogoutAsync()
```

**Related BRs:** BR-002, BR-013

---

### FR-002: Event Management (CRUD)

**Requirement:** An Administrator shall create, view, edit, cancel and delete events, and assign
venues and activities to them.

**Details:**

1. Create/edit via a validated form (`EventViewModel`); end time must be after start time (BR-003).
2. Venues and activities are selected through many-to-many pickers.
3. Listing shows all events; each can be edited, cancelled or deleted.
4. Events are created through the domain constructor so they are always valid.

**Service & Methods:**

```
IEventService:
  Task<List<Event>> GetEventsAsync()
  Task<Event>       GetEventByIdAsync(Guid id)            // throws EventNotFoundException
  Task              CreateEventAsync(EventViewModel vm)
  Task              UpdateEventAsync(EventViewModel vm)
  Task              CancelEventAsync(Guid id, string reason)
  Task              DeleteEventAsync(Guid id)
```

**Related BRs:** BR-001, BR-003, BR-014, BR-015

---

### FR-003: Venue Management (CRUD)

**Requirement:** An Administrator shall manage the catalogue of venues.

**Details:**

1. A venue has Name, Address, Capacity and an Accessibility flag.
2. Capacity is validated as a positive number.
3. Venues can be linked to many events.

**Service & Methods:**

```
IVenueService:
  Task<List<Venue>> GetAllAsync()
  Task<Venue>       GetByIdAsync(Guid id)
  Task              CreateAsync(VenueViewModel vm)
  Task              UpdateAsync(VenueViewModel vm)
  Task              DeleteAsync(Guid id)
```

**Related BRs:** BR-004, BR-014

---

### FR-004: Activity Management (Typed, CRUD)

**Requirement:** An Administrator shall manage activities, choosing one of three types whose
type-specific fields appear conditionally.

**Details:**

1. Type is Workshop, Game or Talk; the create form shows only the chosen type's fields.
2. The service builds the correct subclass and persists it using Table-Per-Hierarchy.
3. Each type renders its own description via the overridden `GetActivityDetails()`.

**Service & Methods:**

```
IActivityService:
  Task<List<Activity>> GetAllAsync()
  Task<Activity>       GetByIdAsync(Guid id)
  Task                 CreateAsync(ActivityViewModel vm)   // builds Workshop/Game/Talk
  Task                 UpdateAsync(ActivityViewModel vm)
  Task                 DeleteAsync(Guid id)
```

**Related BRs:** BR-005, BR-014

---

### FR-005: Participant Management (CRUD)

**Requirement:** An Administrator shall manage participant records.

**Details:**

1. A participant has First Name, Last Name, Email (unique) and Phone Number.
2. E-mail uniqueness is validated and enforced by a database index.
3. `GetByEmailAsync` links a self-service account to its participant.

**Service & Methods:**

```
IParticipantService:
  Task<List<Participant>> GetAllAsync()
  Task<Participant>       GetByIdAsync(Guid id)
  Task<Participant?>      GetByEmailAsync(string email)
  Task                    CreateAsync(ParticipantViewModel vm)
  Task                    UpdateAsync(ParticipantViewModel vm)
  Task                    DeleteAsync(Guid id)
```

**Related BRs:** BR-006, BR-012

---

### FR-006: Browse & Filter Events

**Requirement:** Any user shall browse upcoming events and filter them by search term, date, venue
and activity type.

**Details:**

1. The browse page shows upcoming, non-cancelled events as cards.
2. A **400 ms debounced** search box avoids a query on every keystroke.
3. Filters are combined dynamically — only the chosen filters are added to the query (`IQueryable`).
4. Activity-type filtering uses the TPH discriminator (`EF.Property<string>(a, "ActivityType")`).

**Service & Methods:**

```
IEventService (method overloading):
  Task<List<Event>> GetEventsAsync()                                   // all
  Task<List<Event>> GetEventsAsync(DateTime date)                      // by date
  Task<List<Event>> GetEventsAsync(string? term, DateTime? date,
                                   Guid? venueId, string? activityType) // flexible search
  Task<List<Event>> GetUpcomingEventsAsync()
```

**Related BRs:** BR-001, BR-002

---

### FR-007: Event Registration (Self-Service)

**Requirement:** A Registered User shall register themselves for an event; an Administrator may
register a chosen participant.

**Details:**

1. Registration enforces, in order: event exists → event not cancelled → participant exists → not
   already registered → seats available.
2. Each broken rule raises a specific custom exception turned into a friendly message.
3. A Registered User registers only themselves (resolved from the signed-in identity); an
   Administrator uses a participant picker.

**Service & Methods:**

```
IRegistrationService:
  Task<Registration> RegisterAsync(Guid eventId, Guid participantId)
      // throws EventNotFoundException, EntityNotFoundException,
      //        EventManagementException, DuplicateRegistrationException,
      //        VenueCapacityExceededException
```

**Related BRs:** BR-007, BR-008, BR-009

---

### FR-008: View & Cancel My Registrations

**Requirement:** A Registered User shall view and cancel their own registrations.

**Details:**

1. The "My Registrations" page lists the signed-in user's bookings.
2. Cancelling performs a soft cancel (status → Cancelled), freeing a seat.

**Service & Methods:**

```
IRegistrationService:
  Task<List<Registration>> GetByParticipantAsync(Guid participantId)
  Task                     CancelRegistrationAsync(Guid registrationId, string reason)
```

**Related BRs:** BR-011

---

### FR-009: Event Cancellation

**Requirement:** An Administrator shall cancel an event with a recorded reason.

**Details:**

1. Cancelling sets the event's cancelled flag and reason (soft cancel via `ICancelable`).
2. Cancelled events can no longer be registered for (FR-007).

**Service & Methods:**

```
IEventService.CancelEventAsync(Guid id, string reason)  // calls Event.Cancel(reason)
```

**Related BRs:** BR-009, BR-010

---

### FR-010: Self-Service Sign-Up

**Requirement:** A Visitor shall create their own account.

**Details:**

1. Sign-up captures full name, e-mail and password (with confirm-password).
2. The system creates a login `User` (BCrypt hash) and a linked `Participant` (same e-mail).
3. Validation enforces e-mail format, password complexity and matching confirmation.

**Service & Methods:**

```
IAccountService.SignUpAsync(SignUpViewModel vm)   // creates User + linked Participant
SignUpViewModelValidator: email, password rules, confirm match
```

**Related BRs:** BR-012, BR-006

---

### FR-011: Administrator Dashboard

**Requirement:** An Administrator shall see headline counts and the next upcoming events.

**Details:**

1. Tiles show counts of Events, Venues, Activities and Participants.
2. A mini-list shows the next few upcoming, non-cancelled events with their registration counts.
3. Quick-action buttons jump to the create forms.

**Service & Methods:**

```
Uses IEventService.GetEventsAsync(), IVenueService.GetAllAsync(),
     IActivityService.GetAllAsync(), IParticipantService.GetAllAsync()
```

**Related BRs:** BR-002

---

### FR-012: In-App Notifications

**Requirement:** The system shall give immediate, non-blocking feedback for user actions.

**Details:**

1. Success/error/info toasts appear top-right and auto-dismiss (~4 s).
2. Implemented with the Observer pattern: the service raises `OnChange`, the `Toaster` subscribes.

**Service & Methods:**

```
IToastService:
  void ShowSuccess(string message)
  void ShowError(string message)
  void ShowInfo(string message)
  event Action OnChange
```

**Related BRs:** BR-002

---

### FR-013: Validation & Error Handling

**Requirement:** The system shall validate all input and convert errors into friendly messages.

**Details:**

1. FluentValidation runs in every EditForm (client + server), including cross-property and
   conditional rules.
2. Domain exceptions are caught by `CustomErrorBoundary`; unhandled errors fall back to `/Error`;
   unknown routes fall back to `/not-found`.

**Service & Methods:**

```
Validators: EventValidator, VenueValidator, ActivityValidator, ParticipantValidator,
            LoginViewModelValidator, SignUpViewModelValidator  (registered as IValidator<T>)
CustomErrorBoundary.OnErrorAsync -> maps exception type to friendly text
```

**Related BRs:** BR-003, BR-007, BR-008, BR-009

---

## NON-FUNCTIONAL REQUIREMENTS (NFRs)

### NFR-001: Performance

- Search is **debounced (400 ms)** and builds an `IQueryable`, so only the selected filters are sent
  to the database.
- All data access is fully `async`/`await`.
- A fresh, short-lived `DbContext` per operation (via `IDbContextFactory`) avoids contention on a
  Blazor circuit.

### NFR-002: Security

- Cookie authentication; **BCrypt**-hashed, salted passwords; 8-hour cookie expiry.
- Role-based authorisation enforced in navigation and pages.
- Anti-forgery middleware enabled; HTTPS redirection; generic login failure messages.
- Parameterised queries via EF Core (no string-concatenated SQL).

### NFR-003: Reliability & Availability

- Custom exception hierarchy + `CustomErrorBoundary` keep the app stable and user-friendly under
  error conditions.
- Optimistic concurrency token prevents silent lost updates.
- Database is migrated and seeded automatically on startup.

### NFR-004: Usability & Accessibility

- Professional SaaS-grade UI: violet/indigo design system, reusable components (PageHeader, StatCard,
  StatusBadge, EmptyState, LoadingButton, Skeleton loaders), toasts.
- Responsive from 375 px to desktop; `:focus-visible` rings; WCAG-compliant text contrast;
  `prefers-reduced-motion` respected.

### NFR-005: Maintainability & Testability

- Clean four-layer architecture; dependencies on interfaces only.
- 43 automated tests (xUnit, SQLite in-memory, Moq, bUnit, FluentValidation.TestHelper) plus a
  Playwright end-to-end layer.
- Consistent code style and verbose XML/inline documentation.

### NFR-006: Data Integrity & Validation

- Unique indexes (participant e-mail; `Registration(EventId, ParticipantId)`).
- Business rules enforced inside entities so they cannot be bypassed.
- FluentValidation guards every form.

### NFR-007: Portability

- Runs against MySQL/MariaDB in production and SQLite in-memory for tests, achieved by using
  `.IsConcurrencyToken()` rather than provider-specific `.IsRowVersion()`.

---

## TECHNOLOGY STACK

| Area | Technology |
|------|------------|
| Framework / language | .NET 10, C# 13 |
| UI | Blazor Web App (Interactive Server) |
| ORM | Entity Framework Core 9 |
| Database | MySQL / MariaDB (Pomelo provider); SQLite in-memory for tests |
| Auth | ASP.NET Core Cookie Authentication + BCrypt.Net-Next |
| Validation | FluentValidation + Blazored.FluentValidation |
| Testing | xUnit, SQLite, Moq, bUnit, FluentValidation.TestHelper, Microsoft.Playwright |

---

## DATABASE SCHEMA (SUMMARY)

Eight tables are generated (six entity tables + two M:N junctions). See `CEMS_UML_v1.0.md` §5 for the
full ERD.

| Table | Notes |
|-------|-------|
| `Events` | Core event record; soft-cancel fields; concurrency token |
| `Participants` | Unique `Email`; linked to a self-service `User` by e-mail |
| `Venues` | Capacity + accessibility |
| `Activities` | **TPH** — `ActivityType` discriminator + nullable subclass columns |
| `Registrations` | Join entity; **unique index** on `(EventId, ParticipantId)` |
| `Users` | Login accounts; `Role`; BCrypt `PasswordHash` |
| `EventVenues` | M:N junction (Event ↔ Venue) |
| `EventActivities` | M:N junction (Event ↔ Activity) |

---

## REQUIREMENT TRACEABILITY MATRIX

| FR | Business Rules | Primary components / services |
|----|----------------|-------------------------------|
| FR-001 | BR-002, BR-013 | `AuthController`, `AuthService`, `Login.razor` |
| FR-002 | BR-001, BR-003, BR-014, BR-015 | `EventService`, Admin event pages |
| FR-003 | BR-004, BR-014 | `VenueService`, Admin venue pages |
| FR-004 | BR-005, BR-014 | `ActivityService`, Admin activity pages |
| FR-005 | BR-006, BR-012 | `ParticipantService`, Admin participant pages |
| FR-006 | BR-001, BR-002 | `EventService` (overloads), `User/EventList.razor` |
| FR-007 | BR-007, BR-008, BR-009 | `RegistrationService`, `User/EventDetail.razor` |
| FR-008 | BR-011 | `RegistrationService`, `User/MyRegistrations.razor` |
| FR-009 | BR-009, BR-010 | `EventService.CancelEventAsync` |
| FR-010 | BR-012, BR-006 | `AccountService`, `SignUp.razor` |
| FR-011 | BR-002 | `Admin/Dashboard.razor` |
| FR-012 | BR-002 | `ToastService`, `Toaster.razor` |
| FR-013 | BR-003, BR-007, BR-008, BR-009 | Validators, `CustomErrorBoundary` |

---

**Document Version:** 1.0 &nbsp;|&nbsp; **Status:** ✅ FINALISED &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
&nbsp;|&nbsp; **Module:** CET254 Advanced Programming &nbsp;|&nbsp; **See also:** `CEMS_BRS_v1.0.md`
