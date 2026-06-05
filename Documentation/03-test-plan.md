# Test Plan and Results

All **43 automated tests pass**. They use xUnit and are spread across several styles so each layer
is tested in the most appropriate way, covering happy paths, edge cases and boundary conditions.

| Test group | Style | Count | What it covers |
|------------|-------|-------|----------------|
| Entity rules | Plain xUnit | 8 | Duplicate registration, capacity reached exactly (boundary), re-registering after a cancellation (edge case), available-seat counting, cancellation, collection de-duplication |
| Polymorphism | Plain xUnit | 4 | `GetActivityDetails()` dispatched through base-type references to each subclass |
| Validators | FluentValidation TestHelper | 10 | Cross-property end-time rule, conditional per-subclass rules, boundary capacities, invalid email, empty password |
| Repositories | SQLite in-memory | 7 | Save with links, unknown id, filter by date / venue / activity-type (TPH discriminator), unique-index violation, cascade delete |
| Services | Moq | 11 | Register happy path, duplicate, capacity, cancelled event, event/participant not found, cancel event, cancel registration |
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

Expected output: `Passed! - Failed: 0, Passed: 43`.

> Add a screenshot of the Visual Studio Test Explorer (all green) here as evidence in the final
> submission document.
