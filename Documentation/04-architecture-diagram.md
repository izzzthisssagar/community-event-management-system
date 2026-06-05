# Layered Architecture Diagram

Shows the four layers and their dependencies. The Application layer depends on the repository
*interfaces* in the Domain layer, and the Infrastructure layer *implements* them (dashed arrow) —
the Repository pattern with dependency inversion.

```mermaid
flowchart TB
    subgraph PR["Presentation Layer"]
        Pages["Blazor Components<br/>(Admin CRUD, Browse, Register)"]
        Auth["AuthController (MVC)"]
        EB["CustomErrorBoundary"]
    end
    subgraph AP["Application Layer"]
        Services["Services + Interfaces"]
        Valid["FluentValidation Validators"]
    end
    subgraph DOM["Domain Layer"]
        Entities["Entities + BaseEntity<br/>ICancelable, Activity hierarchy"]
        RepoIf["Repository Interfaces"]
        Exc["Custom Exception Hierarchy"]
    end
    subgraph INF["Infrastructure Layer"]
        Repos["Repository Implementations"]
        Ctx["ApplicationDbContext<br/>(IDbContextFactory)"]
        Cfg["Fluent API Configurations"]
    end
    DB[("MySQL Database")]

    Pages --> Services
    Pages --> Valid
    Auth --> Services
    Services --> RepoIf
    Services --> Entities
    Services --> Exc
    Repos -. implements .-> RepoIf
    Repos --> Ctx
    Ctx --> Cfg
    Ctx --> DB
    EB -. catches .-> Exc
```
