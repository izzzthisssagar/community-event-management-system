# COMMUNITY EVENT MANAGEMENT SYSTEM (CEMS)

## BUSINESS REQUIREMENTS SPECIFICATION (BRS) v1.0

**Document Status:** ✅ FINALISED &nbsp;|&nbsp; **Release:** v1.0 &nbsp;|&nbsp; **Last Updated:** June 8, 2026
**Module:** CET254 Advanced Programming — Assignment 1 &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
**Classification:** Academic Submission — University of Sunderland

---

## EXECUTIVE SUMMARY

The Community Event Management System (CEMS) is a web-based application that enables a community
organisation to plan and run events, manage the venues and activities those events use, register
participants and track their bookings. It serves a back-office **Administrator** who manages all
data, and self-service **Registered Users** (members of the public) who create their own account,
browse upcoming events and register themselves.

**Key Objectives**

- Manage the full lifecycle of community events (create, edit, schedule, cancel).
- Maintain a catalogue of venues and a typed catalogue of activities (workshops, games, talks).
- Let members of the public self-register for events and manage their own registrations.
- Enforce the business rules that protect data integrity (no duplicate bookings, no overbooking,
  no booking a cancelled event).
- Provide secure, role-based access with hashed passwords.
- Give clear, friendly feedback for both success and error conditions.

**Scope (v1.0)**

- ✅ Event, Venue, Activity and Participant management (full CRUD) by an Administrator.
- ✅ Typed activities via inheritance (Workshop / Game / Talk).
- ✅ Many-to-many assignment of venues and activities to events.
- ✅ Self-service public sign-up, browse, filter, register and cancel.
- ✅ Role-based access control (Administrator, Registered User, Visitor).
- ✅ Cookie authentication with BCrypt-hashed passwords.
- ✅ Soft cancellation (events and registrations keep their history).
- ✅ Optimistic concurrency and audit timestamps on every record.

**Out of Scope (v1.0)**

- ❌ Online payment / ticketing.
- ❌ E-mail / SMS notifications (in-app toasts only).
- ❌ Public REST/Web API for third parties.
- ❌ Multi-tenant / multi-organisation support.
- ❌ File/image uploads for events or venues.

---

## BUSINESS RULES (BRs)

### BR-001: Event Record Management

**Rule:** Every event must be a complete, uniquely identified record created through controlled
domain logic.

**Details:**

- An event holds: Name, Date, Start Time, End Time, Description, Maximum Capacity.
- An event is created only through its public constructor, so it can never exist in a half-built,
  invalid state.
- Each event has a system-generated `Guid` identity and audit timestamps.

**Implementation:**

```
Events table:
- Id (PK, Guid)
- Name, Date, StartTime, EndTime, Description, MaxCapacity
- IsCancelled, CancellationReason
- CreatedAt, UpdatedAt, ConcurrencyToken
```

**Affected:** FR-002

---

### BR-002: Role-Based Access Control (RBAC)

**Rule:** The system enforces role-based permissions; the back office and the public self-service
area are strictly separated.

**Roles & Permissions:**

| Capability | Administrator | Registered User | Visitor |
|------------|:------------:|:---------------:|:-------:|
| Manage Events / Venues / Activities / Participants | ✅ Full CRUD | ❌ | ❌ |
| Cancel an event | ✅ | ❌ | ❌ |
| View Admin Dashboard | ✅ | ❌ | ❌ |
| Browse & filter events | ✅ | ✅ | ✅ |
| Register **self** for an event | ❌ (uses picker) | ✅ | ❌ |
| View / cancel **own** registrations | — | ✅ | ❌ |
| Create an account | — | — | ✅ |

**Details:**

- Roles are stored on the `User` record and read back from the authentication cookie.
- The navigation menu and each page adapt to the signed-in role via cascading `AuthenticationState`.
- Administrators register participants through a picker; Registered Users register only themselves.

**Implementation:**

```
Users table: Role (e.g. "Administrator", "User")
- Cookie authentication claims carry the role
- NavMenu + pages branch on the role
```

**Affected:** FR-001, FR-006, FR-007

---

### BR-003: Event Time Validity (Cross-Field Rule)

**Rule:** An event's End Time must be later than its Start Time.

**Details:**

- This is a **cross-property** validation rule, not a single-field check.
- It is enforced before any event is saved or updated.

**Implementation:**

```
EventValidator (FluentValidation):
- RuleFor(e => e.EndTime).GreaterThan(e => e.StartTime)
```

**Affected:** FR-002, FR-013

---

### BR-004: Venue Management

**Rule:** Each venue is a unique record describing where events can be held, including its capacity
and accessibility.

**Details:**

- A venue holds: Name, Address, Capacity, IsAccessible (step-free access flag).
- Venues can be linked to many events (many-to-many).

**Implementation:**

```
Venues table: Id (PK), Name, Address, Capacity, IsAccessible, CreatedAt, UpdatedAt
```

**Affected:** FR-003, FR-002

---

### BR-005: Typed Activities (Workshop / Game / Talk)

**Rule:** An activity must be one of three concrete types, each with its own type-specific fields and
its own way of describing itself.

**Details:**

- `Activity` is abstract; the three concrete types are **Workshop**, **Game** and **Talk**.
- Each type adds its own fields and overrides `GetActivityDetails()` to format itself.
  - Workshop → Instructor Name, Materials Required.
  - Game → Minimum Age, Equipment Provided.
  - Talk → Speaker Name, Topic.
- All three are stored in one table using the Table-Per-Hierarchy strategy with an `ActivityType`
  discriminator.

**Implementation:**

```
Activities table (TPH):
- Id (PK), ActivityType (discriminator), Title, DurationMinutes
- InstructorName, MaterialsRequired   (Workshop, nullable)
- MinimumAge, EquipmentProvided       (Game, nullable)
- SpeakerName, Topic                  (Talk, nullable)
```

**Affected:** FR-004, FR-002

---

### BR-006: Participant Management & Unique E-mail

**Rule:** Each participant is a unique person identified by a unique e-mail address.

**Details:**

- A participant holds: First Name, Last Name, Email, Phone Number.
- The e-mail address is unique across all participants (enforced by a unique index).
- The e-mail is also the link between a self-service `User` account and its `Participant` record.

**Implementation:**

```
Participants table: Id (PK), FirstName, LastName, Email (UNIQUE), PhoneNumber, CreatedAt, UpdatedAt
```

**Affected:** FR-005, FR-010

---

### BR-007: No Duplicate Registration

**Rule:** A participant cannot hold more than one **active** registration for the same event.

**Details:**

- The check counts only active (non-cancelled) registrations, so a participant who cancelled may
  re-register.
- The rule is enforced both in domain logic and by a database constraint (defence in depth).

**Implementation:**

```
Event.AddRegistration(...):
  if participant already has an active registration -> throw DuplicateRegistrationException
Registrations table: UNIQUE index on (EventId, ParticipantId)
```

**Affected:** FR-007

---

### BR-008: Event Capacity Limit

**Rule:** The number of active registrations for an event must never exceed its Maximum Capacity.

**Details:**

- When `MaxCapacity` is greater than zero and is already reached, no further registration is
  accepted.
- `GetAvailableSeats()` reports the remaining seats for display.

**Implementation:**

```
Event.AddRegistration(...):
  if activeRegistrations >= MaxCapacity -> throw VenueCapacityExceededException
```

**Affected:** FR-007

---

### BR-009: No Registration for a Cancelled Event

**Rule:** Once an event is cancelled, nobody may register for it.

**Details:**

- The cancelled state is checked before the registration rules are applied.

**Implementation:**

```
RegistrationService.RegisterAsync(...):
  if event.IsCancelled -> throw EventManagementException("...has been cancelled")
```

**Affected:** FR-007, FR-009

---

### BR-010: Event Cancellation (Soft Cancel)

**Rule:** Events are cancelled, never silently deleted; a cancellation always records a reason.

**Details:**

- Cancelling sets `IsCancelled = true`, stores the reason and updates the audit timestamp.
- The event remains in the system for history and reporting.
- Implemented through the `ICancelable` interface (shared with registrations).

**Implementation:**

```
Event : ICancelable -> Cancel(string reason) { IsCancelled = true; CancellationReason = reason; }
```

**Affected:** FR-009

---

### BR-011: Registration Cancellation (Soft Cancel)

**Rule:** A registration can be cancelled by its owner; cancellation is recorded, not deleted.

**Details:**

- Cancelling a registration sets `IsCancelled = true`, sets `Status = "Cancelled"` and stores the
  reason.
- This frees a seat (cancelled registrations are not counted towards capacity).
- Implemented through the **same** `ICancelable` interface as events, but with **different**
  behaviour — a demonstration of interface polymorphism.

**Implementation:**

```
Registration : ICancelable -> Cancel(string reason) { IsCancelled = true; Status = "Cancelled"; }
```

**Affected:** FR-008

---

### BR-012: Self-Service Account Creation

**Rule:** A member of the public can create their own account, which produces both a login `User`
and a linked `Participant`.

**Details:**

- Sign-up captures name, e-mail and password.
- The system creates a `User` (for login) and a matching `Participant` (for registrations), linked
  by e-mail.
- E-mail must be unique and the password must meet the complexity rule.

**Implementation:**

```
AccountService.SignUpAsync(vm):
  create User (BCrypt hash) + create Participant (same email)
SignUpViewModelValidator: email format, password rules, confirm-password match
```

**Affected:** FR-010, FR-006

---

### BR-013: Authentication & Password Security

**Rule:** Access to protected areas requires authentication; passwords are never stored in plain
text.

**Details:**

- Login is by e-mail + password; the password is verified against a **BCrypt** hash.
- A successful login issues an authentication cookie (8-hour lifetime).
- Failed logins return a generic message (no account enumeration).

**Implementation:**

```
AuthService: BCrypt.Verify(password, user.PasswordHash)
Program.cs: AddAuthentication(Cookie) ... ExpireTimeSpan = 8h
Users table: PasswordHash (BCrypt)
```

**Affected:** FR-001

---

### BR-014: Event ↔ Venue and Event ↔ Activity Assignment

**Rule:** An event can be held at several venues and can include several activities; a venue or
activity can be reused across many events.

**Details:**

- Both relationships are many-to-many, resolved by junction tables.
- Adding the same venue/activity twice is prevented inside the `Event` entity.

**Implementation:**

```
EventVenues (EventsId, VenuesId) ; EventActivities (EventsId, ActivitiesId)
Event.AddVenue / AddActivity guard against duplicates
```

**Affected:** FR-002, FR-003, FR-004

---

### BR-015: Data Integrity, Auditing & Concurrency

**Rule:** Every record is uniquely identified, time-stamped and protected against concurrent
overwrites.

**Details:**

- Every entity inherits `Id (Guid)`, `CreatedAt`, `UpdatedAt` and a `ConcurrencyToken`.
- The concurrency token causes a conflicting save to be rejected (optimistic concurrency).
- The token uses `.IsConcurrencyToken()` so it works on both MySQL and the SQLite test database.

**Implementation:**

```
BaseEntity: Id, CreatedAt, UpdatedAt, ConcurrencyToken
Fluent API: builder.Property(e => e.ConcurrencyToken).IsConcurrencyToken()
```

**Affected:** FR-002, FR-003, FR-004, FR-005

---

## SUMMARY TABLE: 15 BUSINESS RULES

| # | Rule | Category | Status |
|---|------|----------|:------:|
| BR-001 | Event Record Management | Domain | ✅ Final |
| BR-002 | Role-Based Access Control | Security | ✅ Final |
| BR-003 | Event Time Validity (cross-field) | Validation | ✅ Final |
| BR-004 | Venue Management | Domain | ✅ Final |
| BR-005 | Typed Activities (Workshop/Game/Talk) | Domain | ✅ Final |
| BR-006 | Participant Management & Unique E-mail | Domain | ✅ Final |
| BR-007 | No Duplicate Registration | Integrity | ✅ Final |
| BR-008 | Event Capacity Limit | Integrity | ✅ Final |
| BR-009 | No Registration for a Cancelled Event | Integrity | ✅ Final |
| BR-010 | Event Cancellation (soft) | Domain | ✅ Final |
| BR-011 | Registration Cancellation (soft) | Domain | ✅ Final |
| BR-012 | Self-Service Account Creation | Domain | ✅ Final |
| BR-013 | Authentication & Password Security | Security | ✅ Final |
| BR-014 | Event ↔ Venue / Activity Assignment | Domain | ✅ Final |
| BR-015 | Data Integrity, Auditing & Concurrency | Integrity | ✅ Final |

---

**Document Version:** 1.0 &nbsp;|&nbsp; **Status:** ✅ FINALISED &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
&nbsp;|&nbsp; **Module:** CET254 Advanced Programming &nbsp;|&nbsp; **See also:** `CEMS_SRS_v1.0.md`
