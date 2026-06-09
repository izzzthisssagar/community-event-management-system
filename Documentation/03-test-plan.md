# Test Plan and Results

All **93 automated tests pass**. They use xUnit and are spread across several styles so each layer
is tested in the most appropriate way, covering happy paths, edge cases and boundary conditions.

| Test group | Style | Count | What it covers |
|------------|-------|-------|----------------|
| Entity rules | Plain xUnit | 15 | Duplicate registration, capacity reached exactly (boundary), re-registering after cancellation (edge), available-seat counting, all-cancelled seats freed (edge), **Cancel() idempotency** (boundary), UpdatedAt set on cancel, activity and venue deduplication, venue removal |
| Polymorphism | Plain xUnit | 10 | `GetActivityDetails()` dispatched through base-type references to each subclass; each subclass surfaces its own specific fields; DurationMinutes on base class; IsAssignableFrom hierarchy check |
| Validators | FluentValidation TestHelper | 30 | Cross-property end-time rule, conditional per-subclass rules, boundary capacities, name/description empty, date today (boundary), start==end time (boundary), negative capacity, duration zero, **SignUpViewModelValidator** (BCrypt 72-char max boundary, min-length boundary, passwords-match cross-property, invalid email, empty name), LoginValidator |
| Repositories | SQLite in-memory | 15 | Save with links, unknown id, filter by date / venue / activity-type (TPH), unique-index violation, cascade delete, GetUpcomingAsync excludes cancelled and past (edge), includes today (boundary), GetAllAsync, SearchAsync by term and no-filters, UpdateAsync, SaveCancellationAsync |
| Event service | Moq | 12 | All three GetEventsAsync overloads, GetUpcomingEventsAsync, CreateEventAsync, UpdateEventAsync (no Id, capacity below active registrations, capacity exactly equals active count boundary), DeleteEventAsync, CancelEventAsync when event not found |
| Registration services | Moq | 8 | Register happy path, duplicate, capacity, cancelled event, event/participant not found, cancel registration, cancel when not found |
| Components | bUnit | 3 | Browse list renders cards, detail shows name, register-without-participant shows error |

## Why these testing methods

- **SQLite in-memory** (not the EF "InMemory" provider) is a real relational database, so the
  repository tests actually enforce the foreign keys and the unique index — the same rules MySQL
  enforces in production. Writing these tests even caught a real cross-provider bug (ordering by a
  `TimeSpan` column).
- **Moq** isolates the service business logic from the database so the rules can be tested directly.
- **bUnit** renders the Blazor components in memory to check the UI behaves correctly.
- **FluentValidation TestHelper** checks the validation rules, including the cross-property and
  conditional rules.

## How to run the tests

```bash
dotnet test
```

Expected output: `Passed! - Failed: 0, Passed: 93`.

> Add a screenshot of the Visual Studio Test Explorer (all green) here as evidence in the final
> submission document.
