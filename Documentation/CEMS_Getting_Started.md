# COMMUNITY EVENT MANAGEMENT SYSTEM (CEMS)

## GETTING STARTED — From Localhost to Testing (Step by Step)

**Last Updated:** June 9, 2026 &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss) &nbsp;|&nbsp; **Module:** CET254

This guide takes you from a fresh machine all the way to running the app **and** all three test
suites (unit, Selenium, Playwright). Commands are shown for **PowerShell** (the default Windows
terminal). Where the syntax differs, a note is given for Git Bash / cmd.

---

## 0. WHAT YOU NEED (one-time setup)

| Tool | Why | Check it's installed |
|------|-----|----------------------|
| **.NET 10 SDK** | builds & runs the app and the tests | `dotnet --version` → `10.0.x` |
| **XAMPP (MySQL/MariaDB)** | the database | open XAMPP Control Panel |
| **Google Chrome** | the Selenium & Playwright tests drive a real Chrome | open Chrome |
| **Git** (optional) | clone the repo | `git --version` |

> You do **not** need to download ChromeDriver. Selenium 4.6+ downloads the right driver for your
> installed Chrome automatically the first time.

---

## 1. GET THE CODE

If you have it as a folder already, just open a terminal in the project root
(`...\Assignment_Programming`). Otherwise clone it:

```powershell
git clone https://github.com/izzzthisssagar/community-event-management-system.git
cd community-event-management-system
```

The root contains:

```
CommunityEventManagement/            ← the web app
CommunityEventManagement.Tests/      ← 43 unit/integration/component tests
CommunityEventManagement.SeleniumTests/   ← Selenium browser tests  (this guide §6)
CommunityEventManagement.E2ETests/        ← Playwright browser tests (this guide §7)
CommunityEventManagement.slnx        ← the solution file
Documentation/                       ← all the written documents
```

---

## 2. START THE DATABASE

1. Open the **XAMPP Control Panel**.
2. Click **Start** next to **MySQL**. (Apache is not needed.)
3. Leave it running.

The connection string lives in `CommunityEventManagement/appsettings.json`. The default expects MySQL
on `localhost` with the standard XAMPP root user and no password. If your MySQL is different, edit the
`DefaultConnection` value there.

---

## 3. RUN THE APP ON LOCALHOST

From the project root:

```powershell
dotnet run --project CommunityEventManagement --launch-profile http
```

- On the **first run** the database is created and **seeded** automatically with the two demo
  accounts plus **10+ sample records per entity** (11 events, 10 venues, 11 activities, 10 participants).
- When you see `Now listening on: http://localhost:5131`, open that address in Chrome:

  **http://localhost:5131**

> Keep this terminal open — the app must stay running for the browser tests in §6 and §7.

---

## 4. LOG IN AND CLICK AROUND

| Role | Email | Password | What you can do |
|------|-------|----------|-----------------|
| **Admin** | `admin@events.com` | `Admin123!` | manage events, venues, activities, participants |
| **User** | `user@events.com` | `User123!` | browse, filter, register yourself, view/cancel your bookings |

You can also click **Create an account** on the login page to make a brand-new user.

Things to try: the admin dashboard, create an event, then log in as the user, **Browse Events**, use
the search box (notice the 400 ms debounce) and the date / venue / type filters, open an event and
**Register me for this event**.

> **Password visibility:** both the login page and the sign-up page have an eye icon inside the
> password field. Click it to reveal or hide the password you are typing.

---

## 5. RUN THE UNIT TESTS (no browser needed)

Open a **second** terminal in the project root (you can leave the app running or stop it — these
tests use their own in-memory database):

```powershell
dotnet test CommunityEventManagement.Tests
```

Expected:

```
Passed!  - Failed: 0, Passed: 93, Skipped: 0
```

These are the 93 xUnit / SQLite-in-memory / Moq / bUnit / FluentValidation tests.

---

## 6. RUN THE SELENIUM TESTS (real Chrome)

The Selenium tests open Chrome and drive the **running** app, so:

1. **Make sure the app is running** (§3) at `http://localhost:5131`.
2. In a second terminal, set the opt-in flag and run just the Selenium project:

```powershell
$env:RUN_SELENIUM = "1"
dotnet test CommunityEventManagement.SeleniumTests
```

> **Git Bash:** `RUN_SELENIUM=1 dotnet test CommunityEventManagement.SeleniumTests`
> **cmd:** `set RUN_SELENIUM=1 && dotnet test CommunityEventManagement.SeleniumTests`

What happens:

- The first run, Selenium Manager downloads the matching ChromeDriver (needs internet once).
- Chrome runs **headless** (no visible window) by default.
- **To watch the browser drive itself** (great while learning): open
  `CommunityEventManagement.SeleniumTests/SeleniumTestBase.cs` and comment out this line, then re-run:

  ```csharp
  coOptions.AddArgument("--headless=new");
  ```

The 6 Selenium scenarios: admin login → dashboard, event-form validation, browse & filter, register
for an event, duplicate-registration error, and sign-up password validation.

> **Why was nothing run before I set `RUN_SELENIUM=1`?** The tests are marked `[SeleniumFact]`, which
> **skips** them unless `RUN_SELENIUM=1`. That is deliberate: it keeps the normal `dotnet test`
> (and any CI) green without needing a browser or a running server.

---

## 7. RUN THE PLAYWRIGHT TESTS (optional — the other browser tool)

Playwright is an alternative to Selenium (already written in `CommunityEventManagement.E2ETests`).

1. **One-time:** install the Playwright browser binaries (downloads Chromium, needs internet once):

```powershell
pwsh CommunityEventManagement.E2ETests/bin/Debug/net10.0/playwright.ps1 install
```

   (If you don't have `pwsh`, build the project first — `dotnet build CommunityEventManagement.E2ETests`
   — then run the `playwright.ps1` path above with `powershell` instead of `pwsh`.)

2. With the app running, opt in and run:

```powershell
$env:RUN_E2E = "1"
dotnet test CommunityEventManagement.E2ETests
```

---

## 8. RUN EVERYTHING AT ONCE

From the root, `dotnet test` runs the whole solution. By default the browser tests are **skipped**,
so you get a clean result without any setup:

```powershell
dotnet test
# Passed: 93, Skipped: 12   (the 12 are the browser tests, off by default)
```

To include the browser tests, start the app, set **both** flags, then run:

```powershell
$env:RUN_SELENIUM = "1"; $env:RUN_E2E = "1"
dotnet test
```

---

## 9. TROUBLESHOOTING

| Symptom | Fix |
|---------|-----|
| `Unable to connect to any of the specified MySQL hosts` | Start MySQL in XAMPP (§2); check `appsettings.json`. |
| Build error: file is **locked** / `CommunityEventManagement.exe` in use | The app is still running. Stop it (Ctrl+C in its terminal) before rebuilding. |
| Port `5131` already in use | Stop the other instance, or change the port in `Properties/launchSettings.json`. |
| Selenium: `session not created ... browser version` | Update Google Chrome; Selenium Manager will fetch a matching driver next run. |
| Selenium/Playwright tests all say **Skipped** | That's expected — set `RUN_SELENIUM=1` / `RUN_E2E=1` (§6/§7). |
| Browser test fails on a selector | Make sure the app is the current version and seeded; the demo user must not already be registered for the first event (re-seed by dropping the database and re-running §3). |

---

## QUICK REFERENCE

```powershell
# run the app
dotnet run --project CommunityEventManagement --launch-profile http      # http://localhost:5131

# unit tests (no browser)
dotnet test CommunityEventManagement.Tests                               # 93 passed

# selenium tests (app must be running)
$env:RUN_SELENIUM = "1"; dotnet test CommunityEventManagement.SeleniumTests

# playwright tests (app running + 'playwright install' once)
$env:RUN_E2E = "1"; dotnet test CommunityEventManagement.E2ETests

# everything (browser tests skipped unless the flags above are set)
dotnet test
```

---

**Document Version:** 1.0 &nbsp;|&nbsp; **Author:** Sagar Thapa (bi95ss) &nbsp;|&nbsp; **Module:** CET254 Advanced Programming
