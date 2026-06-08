# COMMUNITY EVENT MANAGEMENT SYSTEM (CEMS)

## TEST PLAN & TEST DOCUMENTATION — v1.0

**Document Status:** ✅ FINALISED &nbsp;|&nbsp; **Release:** v1.0 &nbsp;|&nbsp; **Last Updated:** June 8, 2026
**Module:** CET254 Advanced Programming — Assignment 1 &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
**Classification:** Academic Submission — University of Sunderland

---

## 1. INTRODUCTION & TEST STRATEGY

This document describes how the Community Event Management System (CEMS) is tested. Testing is
applied at **four levels**, each with the most appropriate tool, so that every layer is verified the
way it is best verified:

| Level | What it proves | Tool |
|-------|----------------|------|
| **Unit** (domain & validators) | Business rules and validation behave correctly in isolation | xUnit, FluentValidation.TestHelper |
| **Unit** (services) | Service orchestration & exception logic, isolated from the database | xUnit + Moq |
| **Integration** (repositories) | Real SQL behaviour — foreign keys, unique indexes, filters | xUnit + SQLite in-memory |
| **Component** (UI) | Blazor components render and behave correctly | bUnit |
| **End-to-end** (whole app) | Real user journeys through a real browser | **Selenium WebDriver** & **Microsoft.Playwright** |

The first four levels are the **43 automated tests** that ship with the solution; the fifth is the
new **Playwright E2E** layer described in §5. Together they cover happy paths, **edge cases** (e.g.
re-registering after a cancellation) and **boundary conditions** (e.g. capacity reached exactly).

### Why these testing methods

- **SQLite in-memory** (not the EF "InMemory" provider) is a *real* relational engine, so repository
  tests actually enforce foreign keys and the unique index — the same rules MySQL enforces in
  production. (Writing these tests even caught a genuine cross-provider bug: ordering by a `TimeSpan`
  column.)
- **Moq** isolates each service from the database so business/exception logic is tested directly.
- **bUnit** renders Blazor components in memory to assert on real UI output.
- **FluentValidation.TestHelper** checks each rule, including cross-property and conditional rules.
- **Playwright** drives a real browser to confirm the layers work together end-to-end.

---

## 2. TEST ENVIRONMENT

| Item | Value |
|------|-------|
| OS | Windows |
| Runtime | .NET 10 SDK |
| Unit/integration DB | SQLite in-memory (created per test) |
| E2E DB | MySQL/MariaDB (XAMPP) with seeded demo data |
| Demo accounts | Admin `admin@events.com` / `Admin123!` · User `user@events.com` / `User123!` |
| Unit test command | `dotnet test` |
| App run command | `dotnet run --project CommunityEventManagement` |

---

## 3. MANUAL TEST CASES

Legend: **P** = Pass (verified). Steps assume the app is running and seeded.

### 3.1 Authentication & Authorisation

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-01 | Valid admin login | Go to `/login`, enter admin creds, submit | `admin@events.com` / `Admin123!` | Redirected in; admin nav (Management) visible | P |
| TC-02 | Valid user login | Login with user creds | `user@events.com` / `User123!` | Redirected in; only user nav visible | P |
| TC-03 | Invalid password | Login with wrong password | `admin@events.com` / `wrong` | Generic "invalid login" message; not signed in | P |
| TC-04 | Empty fields | Submit blank login form | (blank) | Validation messages shown; no post | P |
| TC-05 | Logout | While signed in, click Logout | — | Cookie cleared; redirected to public area | P |
| TC-06 | Protected route as visitor | Browse to an admin URL while signed out | `/admin/events` | Redirected to `/login` | P |
| TC-07 | Role separation | Sign in as user, attempt admin page | `/admin/events` | Access denied / redirected; user cannot manage | P |

### 3.2 Event Management (Admin)

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-08 | Create event (happy) | Open create form, fill valid data, pick venue+activity, save | Name "Spring Fair", future date, 09:00–12:00, cap 50 | Saved; success toast; appears in list | P |
| TC-09 | End before start (boundary) | Set end time earlier than start, save | 12:00 → 09:00 | Cross-field error "end after start"; not saved | P |
| TC-10 | End == start (boundary) | Set end time equal to start | 09:00 → 09:00 | Validation error; not saved | P |
| TC-11 | Missing required fields | Submit empty create form | (blank) | Field validation errors; not saved | P |
| TC-12 | Edit event | Edit an event, change capacity, save | cap 50 → 80 | Updated; success toast | P |
| TC-13 | Assign venues & activities | Tick multiple venues + activities, save | 2 venues, 2 activities | M:N links persisted; shown on detail | P |
| TC-14 | Delete event | Delete an event from the list | — | Removed; confirmation/toast | P |

### 3.3 Venue / Activity / Participant Management (Admin)

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-15 | Create venue | Fill venue form, save | "Town Hall", cap 200, accessible | Saved; success toast | P |
| TC-16 | Venue capacity ≤ 0 | Enter zero/negative capacity | 0 | Validation error | P |
| TC-17 | Create Workshop | Choose type Workshop; type-specific fields appear; save | Instructor, Materials | Workshop saved (TPH) | P |
| TC-18 | Create Game | Choose type Game | Min age 12, equipment true | Game saved; correct fields | P |
| TC-19 | Create Talk | Choose type Talk | Speaker, Topic | Talk saved; correct fields | P |
| TC-20 | Activity details polymorphism | View an event's activities | mixed types | Each prints its own format | P |
| TC-21 | Create participant | Fill participant form, save | unique email | Saved | P |
| TC-22 | Duplicate participant e-mail | Create participant with existing e-mail | existing email | Unique-email validation/constraint error | P |

### 3.4 Browse & Filter (Any user)

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-23 | Browse upcoming | Open Browse Events | — | Only upcoming, non-cancelled events shown | P |
| TC-24 | Debounced search | Type in the search box | "fair" | Results filter after ~400 ms, not per keystroke | P |
| TC-25 | Filter by date | Pick a date | a seeded date | Only that date's events shown | P |
| TC-26 | Filter by venue | Choose a venue | seeded venue | Only events at that venue | P |
| TC-27 | Filter by activity type | Choose "Workshop" | Workshop | Only events including a workshop (TPH query) | P |
| TC-28 | Combined filters | Apply term + date + type | — | Correctly intersected results | P |
| TC-29 | No results | Filter to an empty set | nonsense term | Friendly EmptyState shown | P |

### 3.5 Registration & My Registrations

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-30 | User self-register (happy) | As user, open event, "Register me" | available event | Success message; booking created | P |
| TC-31 | Duplicate registration (edge) | Register again for same event | already registered | DuplicateRegistrationException → friendly message | P |
| TC-32 | Capacity reached exactly (boundary) | Register the (MaxCapacity+1)-th participant | full event | VenueCapacityExceededException → friendly message | P |
| TC-33 | Register for cancelled event | Try to register for a cancelled event | cancelled event | Blocked with friendly message | P |
| TC-34 | Re-register after cancel (edge) | Cancel booking, then register again | same event | Allowed (cancelled bookings don't block) | P |
| TC-35 | View my registrations | Open My Registrations as user | — | Only the signed-in user's bookings shown | P |
| TC-36 | Cancel a registration | Cancel a booking | — | Soft-cancelled; seat freed; status Cancelled | P |
| TC-37 | Admin registers a participant | As admin on event detail, pick participant | a participant | Booking created via picker | P |

### 3.6 Self-Service Sign-Up

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-38 | Sign up (happy) | Open `/signup`, complete form | new unique email | User + linked Participant created; can log in | P |
| TC-39 | Password mismatch | Enter non-matching confirm password | mismatch | Validation error; not created | P |
| TC-40 | Sign up existing e-mail | Use an e-mail already in use | existing email | Rejected with message | P |

### 3.7 Error Handling & Robustness

| ID | Scenario | Steps | Test data | Expected result | Status |
|----|----------|-------|-----------|-----------------|:------:|
| TC-41 | Unknown event id | Navigate to a non-existent event | bad Guid | EventNotFoundException → friendly page, no stack trace | P |
| TC-42 | Unknown route | Visit a non-existent URL | `/nope` | Friendly `/not-found` page | P |
| TC-43 | Error boundary | Trigger a domain exception in the UI | duplicate reg | CustomErrorBoundary shows friendly message | P |

---

## 4. AUTOMATED TESTS (43) — INVENTORY & MAPPING

All **43 automated tests pass** (`dotnet test` → `Passed: 43, Failed: 0`). They live in
`CommunityEventManagement.Tests/`.

| Test file | Style | Count | Coverage (incl. edge & boundary) |
|-----------|-------|:-----:|----------------------------------|
| `Domain/EventEntityTests.cs` | Plain xUnit | 8 | Duplicate registration, **capacity reached exactly (boundary)**, **re-register after cancel (edge)**, available-seat counting, cancellation, collection de-duplication |
| `Domain/ActivityPolymorphismTests.cs` | Plain xUnit | 4 | `GetActivityDetails()` dispatched through a base-type reference to each subclass |
| `Application/ValidatorTests.cs` | FluentValidation.TestHelper | 10 | Cross-property end-time rule, conditional per-subclass rules, **boundary capacities**, invalid e-mail, empty password |
| `Infrastructure/EventRepositoryTests.cs` | SQLite in-memory | 7 | Save with links, unknown id, filter by date / venue / activity-type (TPH), **unique-index violation**, cascade delete |
| `Application/RegistrationServiceTests.cs` + `EventServiceTests.cs` + `RegistrationServiceEdgeCaseTests.cs` | Moq | 11 | Register happy path, duplicate, capacity, cancelled event, event/participant not found, cancel event, cancel registration |
| `Components/EventListTests.cs` + `EventDetailTests.cs` | bUnit | 3 | Browse list renders cards; detail shows event name; register-without-participant shows the exact error |

**Helper:** `TestHelpers/TestDbContextFactory.cs` builds an isolated SQLite-in-memory `DbContext`
per test.

### Mapping automated tests to requirements

| Requirement | Covered by |
|-------------|-----------|
| BR-007 / FR-007 (no duplicate) | `EventEntityTests`, `RegistrationService*Tests`, `EventRepositoryTests` (unique index) |
| BR-008 / FR-007 (capacity) | `EventEntityTests` (exact boundary), `RegistrationServiceTests` |
| BR-009 (cancelled event) | `RegistrationServiceTests` |
| BR-003 / FR-013 (cross-field) | `ValidatorTests` |
| BR-005 (polymorphism) | `ActivityPolymorphismTests` |
| FR-006 (filters/TPH) | `EventRepositoryTests` |
| FR-002/FR-007 (UI) | `EventListTests`, `EventDetailTests` |

---

## 5. END-TO-END AUTOMATION (PLAYWRIGHT)

A separate, **isolated** project — `CommunityEventManagement.E2ETests` — drives the running
application through a real Chromium browser using **Microsoft.Playwright + xUnit**. It is independent
of the 43-test unit project, so `dotnet test` on the unit suite is unaffected.

### E2E scenarios

| ID | Scenario | Asserts |
|----|----------|---------|
| E2E-01 | Admin logs in | Dashboard loads; admin navigation present |
| E2E-02 | Admin creates an event | Success toast; event appears in the list |
| E2E-03 | User logs in and registers for an event | Booking confirmed; appears in My Registrations |
| E2E-04 | Browse & filter | Filtering by type/date updates the list |
| E2E-05 | Duplicate registration | Friendly error message is shown |
| E2E-06 | Self-service sign-up validation | Mismatched passwords block submission |

### How to run the E2E tests

```bash
# 1. Start MySQL (XAMPP) and run the app in one terminal
dotnet run --project CommunityEventManagement

# 2. First time only: install the Playwright browser binaries
#    (requires internet once; downloads Chromium ~150 MB)
pwsh CommunityEventManagement.E2ETests/bin/Debug/net10.0/playwright.ps1 install

# 3. In a second terminal, run the E2E tests against the running app
dotnet test CommunityEventManagement.E2ETests
```

> **Environment note.** The E2E scripts are the deliverable and are committed regardless of whether
> the browser binaries are installed. They require (a) the app running on its configured URL and
> (b) a one-time `playwright install`, which needs internet access to download Chromium. On an
> offline machine the scripts still compile; they simply skip/await a browser until one is installed.

---

## 5b. END-TO-END AUTOMATION (SELENIUM)

A second isolated project — `CommunityEventManagement.SeleniumTests` — covers the same journeys using
**Selenium WebDriver + xUnit**, as a learning-friendly alternative to Playwright. It is also gated
(`[SeleniumFact]`, skipped unless `RUN_SELENIUM=1`), so it never affects the unit suite or the build.

| ID | Scenario | Asserts |
|----|----------|---------|
| S-01 | Admin logs in | Dashboard ("Quick actions" / "Upcoming events") shown |
| S-02 | Empty event form submitted | Validation messages appear; stays on the form |
| S-03 | Browse & filter | Cards load; filtering by type keeps a healthy page |
| S-04 | User registers for an event | "Registered successfully." shown |
| S-05 | Duplicate registration | Friendly red alert shown, no crash |
| S-06 | Sign-up validation | Mismatched passwords blocked; stays on /signup |

### How to run the Selenium tests

```bash
# 1. Start the app (one terminal)
dotnet run --project CommunityEventManagement

# 2. Opt in and run (second terminal). Selenium Manager fetches ChromeDriver automatically.
#    PowerShell:  $env:RUN_SELENIUM = "1"; dotnet test CommunityEventManagement.SeleniumTests
```

> Chrome runs headless by default; comment out the `--headless=new` line in `SeleniumTestBase.cs` to
> watch the browser. Full from-scratch setup is in `CEMS_Getting_Started.md`.

## 6. HOW TO RUN EVERYTHING

```bash
# Unit / integration / component tests (no app or DB required)
dotnet test CommunityEventManagement.Tests        # -> Passed: 43

# End-to-end via Selenium (app + MySQL + Chrome required, see §5b)
#   PowerShell: $env:RUN_SELENIUM = "1"; dotnet test CommunityEventManagement.SeleniumTests

# End-to-end via Playwright (app + MySQL + browser required, see §5)
#   PowerShell: $env:RUN_E2E = "1"; dotnet test CommunityEventManagement.E2ETests
```

Expected unit result: `Passed!  - Failed: 0, Passed: 43`.

> Attach a screenshot of the Visual Studio Test Explorer (all green) here as evidence in the final
> submission document.

---

**Document Version:** 1.0 &nbsp;|&nbsp; **Status:** ✅ FINALISED &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss)
&nbsp;|&nbsp; **Module:** CET254 Advanced Programming
