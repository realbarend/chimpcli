# CHANGELOG

## Release v2.2 (19-apr-2026)

- add CLAUDE.md 🤖
- enable trimming for smaller executables
  - removed AWSSDK, to prepare for trimming
  - refactored localization: remove use of reflection, to prepare for trimming
  - added json source generation, to prepare for trimming
- improved handling of bad state
- notify user if newer application version is available

## Release v2.1 (29-mar-2026)

- added support for multi-factor authentication (software tokens)

## Release v2.0.1 (24-mar-2026)

- enabled self-contained builds for tags ending in '-self-contained'

## Release v2.0 (23-mar-2026)

- **complete rehaul** of the CLI and its core logic
- **new commands**:
  - `week` (alias `w`) for persistent "time travel" between weeks
  - `copy` (alias `c`) for duplicating existing time entries
  - all commands now have short aliases (l, p, a, u, d, c, w, ls)
- **powerful time parsing**:
  - support for start-end ranges (e.g., `09:00-10:30`), start+duration (e.g., `09:00+45`), and duration only (e.g., `1h30m`)
  - support for weekday prefixes (e.g., `mo:1h`) to track hours on different days
  - 'add' now accepts project, time, and notes in flexible order
- **enhanced UI/UX**:
  - **time overlap detection**: displays a warning icon (⚠) if entries overlap
  - **gap detection**: shows gaps in the daily overview
  - **smart date headers**: context-aware headers (TODAY, weekday names, or full dates)
  - **time traveler alerts**: indicates when you are not viewing the current week
  - **missing notes warning**: displays a warning if an entry has no notes
  - **totals**: weekly and daily totals including billable/non-billable hours
- **localization**: improved English and Dutch support, including localized error messages
- **configuration**: support for environment variables (`CHIMPCLI_USERNAME`, `CHIMPCLI_PASSWORD`, etc.)
- bump to dotnet 10
- **fix**: auth refresh resulted in 'host not found' error
- **fix**: changed output from unicode to utf8 to improve shell compatibility

## Release v1.4 (28-jan-2025)

- **fix**: addressed some minor api changes
- changed login flow to use cognito authentication

## Release v1.3 (17-jan-2024)

- display warning-icon if notes are missing
- use hours-notation in day-summary
- **fix**: login could fail when it encountered an invalid cookie header

## Release v1.2 (30-dec-2023)

- localized and improved most messages
- English fallback for weekday prefix
- no known issues

## Release v1.1 (29-dec-2023)

- added support for adding tags

### Known issues

- errors are not localized

## Release v1.0 (27-dec-2023)

### Known issues

- no yet possible to use labels

- first release
- timechimp login using username and password
- view tracked hours for current week
- view tracked hours for different week
- list available projects
- add, edit, delete tracked hours
- auto-generate binaries in github ci
- language support for English and Dutch (after login)
