# 10-Minute Demonstration Script (OBS / Panopto)

The demonstration is worth 10%. The top band wants a "well-articulated demonstration… leaving no
burning questions and covering all outcomes," explicitly pointing out **where** you used interfaces,
polymorphism, etc. Follow this script and speak to each highlighted point. Keep the app running
(`dotnet run --project CommunityEventManagement`) and Visual Studio open.

> Tip: state the marking area out loud as you show it, e.g. "Here is my **interface polymorphism**…"

## 0:00–1:00 — Intro & running app
- Say your name, ID (bi95ss) and the module.
- Show the app already running. Log in as `admin@events.com` / `Admin123!`.
- Point out the login is **cookie authentication** with a **BCrypt-hashed** password, and that the
  login form posts to an **MVC AuthController** (mention `/auth/test`).

## 1:00–2:30 — Scope & functionality (10%)
- Show the **Admin Dashboard** counts.
- Create an **event**: fill the form, tick a couple of **venues** and **activities** (show the
  many-to-many selection), save. The event appears in the list.
- Browse as a user: open **Browse Events**, type in the search box and point out the **400ms
  debounced search** (one query after you stop typing, not per keystroke).
- Filter by venue and by activity type.

## 2:30–4:00 — OOP: inheritance, interfaces, polymorphism (20%)
- Open `Domain/Entities/BaseEntity.cs` — **abstract base class**; say every entity **inherits** it.
- Open `Activity.cs` — the **abstract class** with `WorkshopActivity`, `GameActivity`,
  `TalkActivity`. Show the abstract `GetActivityDetails()` and the three **overrides**
  (**inheritance polymorphism**). Then in the app, open an event detail and show each activity
  printing its own format.
- Open `ICancelable.cs` — the **interface** implemented by **both** `Event` and `Registration`
  (**interface polymorphism**). Show the two different `Cancel()` implementations.
- Open `EventService.cs` — the three `GetEventsAsync` **method overloads**.
- Open `Event.cs` — point out the **private backing fields** exposed as `IReadOnlyCollection`
  (**encapsulation**) and the **dual constructors** (public + private for EF Core).

## 4:00–5:30 — Data structures & design patterns (20%)
- Open a repository, e.g. `EventRepository.cs` — the **Repository pattern** behind an **interface**,
  using **`IDbContextFactory`** (explain why: safe for Blazor Server).
- Show `EventRepository.SearchAsync` — the **dynamic `IQueryable`** built only from the chosen
  filters, and the **TPH discriminator** query (`EF.Property`).
- Open a Fluent API config, e.g. `EventConfiguration.cs` — the **Fluent API** with the two
  many-to-many relationships, and `ActivityConfiguration.cs` for the **TPH discriminator**.
- Open `Program.cs` — show **dependency injection** registering every interface.

## 5:30–6:30 — Validation & exception handling (10%)
- Open `EventValidator.cs` — the **cross-property** rule (end time after start time). Show it firing
  in the app by entering an end time before the start time.
- Open the `Domain/Exceptions` folder — the **custom exception hierarchy**.
- In the app, register the same participant twice → show the **DuplicateRegistrationException**
  handled gracefully. Open `CustomErrorBoundary.razor` to show the global safety net.

## 6:30–8:30 — Testing (10%)
- Open the Test Explorer, run all tests, show **43 green**.
- Open `EventRepositoryTests.cs` (SQLite in-memory — explain it enforces real FKs/unique indexes),
  `RegistrationServiceTests.cs` (Moq), `ValidatorTests.cs` (boundary + cross-property), and
  `ActivityPolymorphismTests.cs` (calling `GetActivityDetails()` through base references).

## 8:30–9:30 — UML documentation (20%)
- Show the **class diagram**, the **ERD**, the **architecture diagram** and the **sequence diagram**
  from the documentation. Briefly explain the relationships (inheritance, interface realisation,
  the two many-to-many links, the `Registration` association class).

## 9:30–10:00 — Wrap up
- Recap what was shown. Mention the genuine git history (feature branches + pull requests).
- Stop recording, upload to Panopto, and paste the link into `bi95ss_Thapa_Sagar.txt`.
