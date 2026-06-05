# Test Plan and Results

All 13 automated tests pass. They are split across three approaches: repository tests run against
a real in-memory SQLite database, service tests use Moq to isolate the business logic, and
component tests use bUnit to render the Blazor UI.

| #  | Test | What it checks | Expected result | Status |
|----|------|----------------|-----------------|--------|
| 1  | `AddAsync_WithVenueAndActivity_SavesEventAndItsLinks` | Repository (SQLite) | Event is saved with its venue and activity links | Pass |
| 2  | `GetByIdAsync_WhenIdDoesNotExist_ReturnsNull` | Repository (SQLite) | Returns null for an unknown id | Pass |
| 3  | `GetByDateAsync_ReturnsOnlyEventsOnThatDate` | Repository (SQLite) | Only events on the chosen date are returned | Pass |
| 4  | `SearchAsync_FilterByVenue_ReturnsOnlyEventsAtThatVenue` | Repository (SQLite) | Only events at the chosen venue are returned | Pass |
| 5  | `Registration_WithDuplicateEventAndParticipant_ViolatesUniqueIndex` | Repository (SQLite) | The unique index rejects a duplicate registration | Pass |
| 6  | `RegisterAsync_WhenEverythingIsValid_SavesTheRegistration` | Service (Moq) | A valid registration is created and saved once | Pass |
| 7  | `RegisterAsync_WhenParticipantAlreadyRegistered_ThrowsDuplicateRegistrationException` | Service (Moq) | Duplicate registration throws the right exception | Pass |
| 8  | `RegisterAsync_WhenEventIsFull_ThrowsVenueCapacityExceededException` | Service (Moq) | Registering past capacity throws the right exception | Pass |
| 9  | `CancelEventAsync_MarksTheEventAsCancelledWithTheReason` | Service (Moq) | ICancelable.Cancel sets IsCancelled and the reason | Pass |
| 10 | `GetEventsAsync_NoArguments_ReturnsEveryEvent` | Service (Moq) | The no-argument overload returns all events | Pass |
| 11 | `EventList_WhenGivenTwoEvents_RendersTwoEventCards` | Component (bUnit) | The browse page renders one card per event | Pass |
| 12 | `EventDetail_ShowsTheEventName` | Component (bUnit) | The details page shows the event's name | Pass |
| 13 | `EventDetail_ClickingRegisterWithoutChoosingParticipant_ShowsAnError` | Component (bUnit) | A friendly error shows and no registration is attempted | Pass |

## How to run the tests

From the solution folder:

```bash
dotnet test
```

Expected output: `Passed! - Failed: 0, Passed: 13`.

> Add a screenshot of the Visual Studio Test Explorer (all green) here as evidence in the final
> submission document.
