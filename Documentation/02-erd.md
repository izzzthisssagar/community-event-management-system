# Entity Relationship Diagram (ERD) — Database Schema

This ERD shows the actual database tables that Entity Framework Core creates from my model. Note
that all three activity subclasses are stored in the single `ACTIVITIES` table using the
Table-Per-Hierarchy strategy, with an `ActivityType` discriminator column and nullable columns for
the subclass-specific fields. The two many-to-many relationships are resolved by the junction
tables `EVENTVENUES` and `EVENTACTIVITIES`, and the `REGISTRATIONS` table is the join between
events and participants that also carries its own data.

```mermaid
erDiagram
    EVENTS ||--o{ REGISTRATIONS : "has"
    PARTICIPANTS ||--o{ REGISTRATIONS : "makes"
    EVENTS ||--o{ EVENTVENUES : ""
    VENUES ||--o{ EVENTVENUES : ""
    EVENTS ||--o{ EVENTACTIVITIES : ""
    ACTIVITIES ||--o{ EVENTACTIVITIES : ""

    EVENTS {
        char(36) Id PK
        varchar Name
        datetime Date
        time StartTime
        time EndTime
        varchar Description
        int MaxCapacity
        tinyint IsCancelled
        varchar CancellationReason
        datetime CreatedAt
        datetime UpdatedAt
        longblob ConcurrencyToken
    }

    PARTICIPANTS {
        char(36) Id PK
        varchar FirstName
        varchar LastName
        varchar Email UK
        varchar PhoneNumber
        datetime CreatedAt
        datetime UpdatedAt
    }

    VENUES {
        char(36) Id PK
        varchar Name
        varchar Address
        int Capacity
        tinyint IsAccessible
        datetime CreatedAt
        datetime UpdatedAt
    }

    ACTIVITIES {
        char(36) Id PK
        varchar ActivityType "discriminator"
        varchar Title
        int DurationMinutes
        varchar InstructorName "Workshop only, nullable"
        varchar MaterialsRequired "Workshop only, nullable"
        int MinimumAge "Game only, nullable"
        tinyint EquipmentProvided "Game only, nullable"
        varchar SpeakerName "Talk only, nullable"
        varchar Topic "Talk only, nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }

    REGISTRATIONS {
        char(36) Id PK
        char(36) EventId FK
        char(36) ParticipantId FK
        datetime RegistrationDate
        varchar Status
        tinyint IsCancelled
        varchar CancellationReason
        datetime CreatedAt
        datetime UpdatedAt
    }

    USERS {
        char(36) Id PK
        varchar FullName
        varchar Email UK
        longtext PasswordHash
        varchar Role
        datetime CreatedAt
        datetime UpdatedAt
    }

    EVENTVENUES {
        char(36) EventsId FK
        char(36) VenuesId FK
    }

    EVENTACTIVITIES {
        char(36) EventsId FK
        char(36) ActivitiesId FK
    }
```

> Note: `REGISTRATIONS` has a unique index on `(EventId, ParticipantId)` so the same participant
> cannot have two active registrations for the same event.
