# MSSA — Mountain States Stockdog Association

Oqtane website and SQL Server database for a herding trials association. Replaces a
legacy Access database. Not yet in production — the Access migration is still in progress,
so schema changes are still expected and some data is incomplete.

Domain: stockdog (herding) trials. Handlers enter Dogs in Trials held at Events. Runs are
scored by Class. Scores roll up into TopScores and Finals standings for a competition year.

## Stack

- Oqtane 10.2.4 (Blazor). Keep the framework current via System Update; 10.0.3 had an SMTP
  notification bug fixed in 10.2.4.
- SQL Server
- Hosted at WinHost, domain mssadogs.com

## Solution structure — read this before adding files

Three projects: `MountainStates.MSSA.Client`, `.Server`, `.Shared`.

**There are no per-module .csproj files.** Modules are not separate projects. Files live in
flat, per-concern folders and are named or prefixed by the module they belong to:

- `Server/Controllers`, `Server/Manager`, `Server/Repository`, `Server/Startup`
- `Client/Services`
- `Client/Modules/MountainStates.MSSA.Module.<Name>/`
- `Shared/Models`

So a new Entries repository goes in `Server/Repository` alongside every other module's
repository, not in an Entries project. Follow the existing naming when adding anything.

## Modules

Handlers, Dogs, Events, Trials, Entries, TopScores, Finals, BackOffice (score entry),
TrialSecretary (entry workflow).

## Domain rules and gotchas

- `MSSA_Classes` has two rows with `ClassName` "Nursery": `ClassId` 2 and 6. They are
  distinguished by `SubClassName` — On-foot vs Horseback. Never match Nursery on ClassName
  alone.
- Futurity marking: dogs enrolled in Futurity for a competition year are marked with a `+`
  next to the dog name in trial results (Entries, BackOffice, TopScores, Finals). This
  applies **only** to Nursery class scores.
- The Handlers module's membership-checking feature is currently pulled out. There is an
  unresolved table/schema mismatch behind it. Don't reintroduce it without asking.

## Roles and placement

Oqtane roles in use: Host Users, Administrators, and a custom **Trial Secretary** role.

The Events module is placed twice: on a public Calendar page (read-only, visible to
everyone) and on a Manage tab (visible to Administrators and Trial Secretaries). Changes to
Events need to be checked against both placements.

## Theme

Custom "Brown" theme driven by CSS variables set from the MSSA brand colors. Use the
existing variables rather than hardcoding color values.

## Email

SMTP is configured through WinHost — mail server `m01.internetmailserver.net`, sender
`noreply@mssadogs.com`.

Event announcements currently go out through Calendly to a mailing list of roughly 1500
names, sent manually by the admin. The goal is to bring this in-house so that creating an
Event in the Oqtane app sends the announcement automatically. Mailgun is the likely sending
provider.

## Deployment

Deployed to WinHost. Past deployment issues worth remembering:

- DLL placement matters — Oqtane is particular about where module assemblies land.
- `web.config` must block WebDAV; it intercepts requests otherwise.
- HTTP method restrictions in `web.config` need to allow the methods the API uses.

## Build and run

<!-- TODO: fill these in -->
- Build:
- Run locally:
- Connection string lives in:
- How to deploy to WinHost:

## Working preferences

- Explain changes before making them.
- Point out when a change touches the database schema — the Access migration is still
  underway and schema drift is costly right now.
