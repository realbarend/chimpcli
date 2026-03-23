# CHANGELOG

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
