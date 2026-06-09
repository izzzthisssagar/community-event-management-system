# UML Class Diagram — Community Event Management System

This class diagram shows the domain model: the abstract `BaseEntity` base class that every
entity inherits from, the `ICancelable` interface implemented by both `Event` and `Registration`
(interface polymorphism), and the abstract `Activity` class with its three subclasses
(inheritance polymorphism). It also shows the relationships, including the two many-to-many links
and the `Registration` association class.

```mermaid
classDiagram
    direction TB

    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +byte[] ConcurrencyToken
    }

    class ICancelable {
        <<interface>>
        +bool IsCancelled
        +string CancellationReason
        +Cancel(string reason) void
    }

    class Event {
        +string Name
        +DateTime Date
        +TimeSpan StartTime
        +TimeSpan EndTime
        +string Description
        +int MaxCapacity
        +bool IsCancelled
        +string CancellationReason
        +AddRegistration(Participant, string) Registration
        +AddVenue(Venue) void
        +AddActivity(Activity) void
        +Cancel(string reason) void
        +GetAvailableSeats() int
    }

    class Participant {
        +string FirstName
        +string LastName
        +string Email
        +string PhoneNumber
        +string FullName
    }

    class Venue {
        +string Name
        +string Address
        +int Capacity
        +bool IsAccessible
    }

    class Activity {
        <<abstract>>
        +string Title
        +int DurationMinutes
        +GetActivityDetails()* string
    }

    class WorkshopActivity {
        +string InstructorName
        +string MaterialsRequired
        +GetActivityDetails() string
    }

    class GameActivity {
        +int MinimumAge
        +bool EquipmentProvided
        +GetActivityDetails() string
    }

    class TalkActivity {
        +string SpeakerName
        +string Topic
        +GetActivityDetails() string
    }

    class Registration {
        +Guid EventId
        +Guid ParticipantId
        +DateTime RegistrationDate
        +string Status
        +bool IsCancelled
        +string CancellationReason
        +Cancel(string reason) void
    }

    class User {
        +string FullName
        +string Email
        +string PasswordHash
        +string Role
    }

    BaseEntity <|-- Event
    BaseEntity <|-- Participant
    BaseEntity <|-- Venue
    BaseEntity <|-- Activity
    BaseEntity <|-- Registration
    BaseEntity <|-- User

    Activity <|-- WorkshopActivity
    Activity <|-- GameActivity
    Activity <|-- TalkActivity

    ICancelable <|.. Event
    ICancelable <|.. Registration

    Event "1" --> "0..*" Registration : has
    Participant "1" --> "0..*" Registration : makes
    Event "0..*" --> "0..*" Venue : held at
    Event "0..*" --> "0..*" Activity : includes
```

## How to render this diagram

1. Copy everything between the ```mermaid fences (or open this file in an editor that renders
   Mermaid, such as Visual Studio Code with the Mermaid extension, or GitHub).
2. Alternatively paste it into <https://mermaid.live> and export it as a PNG to paste into the
   Word document.
