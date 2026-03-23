# ChimpCLI Functionality Summary

ChimpCLI is a powerful, localized command-line tool for tracking hours in TimeChimp.

## Key Features

### Commands & Aliases
All commands support short aliases for faster typing:
- `login` (alias `l`): Authenticate with TimeChimp using Cognito.
- `projects` (alias `p`): List all available projects and tags with their Short IDs.
- `list` (alias `ls`, or just `chimp`): Display your tracked hours for the active week.
- `add` (alias `a`): Track new hours using project aliases and flexible time formats.
- `update` (alias `u`): Modify notes, time, or project/tags of an existing entry.
- `copy` (alias `c`): Duplicate an existing entry to a new time.
- `delete` (alias `d` or `del`): Remove a time entry.
- `week` (alias `w`): Change the active week for "time travel" mode.
- `help`: Display built-in help.

### Data Entry Formats

#### Project Aliases
Refer to projects and tags using their assigned Short IDs (from the `projects` command):
- `pNN`: Project NN (e.g., `p1`).
- `pNN-A`: Project NN with Tag A (e.g., `p1-2`).
- `pNN-A,B`: Project NN with multiple tags (e.g., `p1-2,5`).

#### Time Entries
ChimpCLI supports a wide variety of time formats:
- **Duration**: `1h30m`, `45m`, `2.5`.
- **Start-End Range**: `09:00-10:30`, `9-1030`, `14.30-16`.
- **Start+Duration**: `09:00+45` (starts at 09:00, lasts 45 minutes).
- **Weekday Prefixes**: Track hours for a specific day by prefixing the time (e.g., `mo:1h30m`, `fr:09:00-10:00`).

### Smart UI & UX
- **Short IDs**: Every entry, project, and tag is assigned a number for easy reference.
- **Time Overlap Detection**: A warning icon (⚠) appears if time entries overlap on the same day.
- **Gap Detection**: Gaps between non-contiguous time entries are clearly marked with `--gap--`.
- **Missing Notes Warning**: Entries without notes are marked with (⚠).
- **Time Travel**: Use the `week` command to persistently view and edit other weeks. A "Time Traveler Alert" reminds you when you're not in the current week.
- **Context-Aware Headers**: Dates are displayed as "TODAY", day names, or full dates depending on the week being viewed.
- **Weekly & Daily Totals**: Automatic calculation of total hours and billable hours.

### Localization
- Full support for **English** and **Dutch**.
- Localized error messages and date formats.
- Automatic detection of UI language from environment or system settings.

### Configuration
- Supports environment variables for headless or secure use:
  - `CHIMPCLI_USERNAME`: Your TimeChimp username.
  - `CHIMPCLI_PASSWORD`: Your TimeChimp password.
  - `CHIMPCLI_LANGUAGE`: Override UI language (`en` or `nl`).
  - `CHIMPCLI_STORE_PASSWORD`: Enable/disable local credential storage.
  - `CHIMPCLI_DEBUG`: Enable detailed logging.
- Data is stored locally in `~/.chimpcli`.
- Built on **.NET 10** with UTF-8 output for high shell compatibility.
