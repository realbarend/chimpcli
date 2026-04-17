# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

ChimpCLI is a .NET 10 command-line interface for the TimeChimp time-tracking service. Users log, view, update, and delete time entries from the terminal. It supports multi-language output (English/Dutch), "time travel" to past weeks, and flexible time-entry syntax.

## Build & run

```bash
dotnet run --project Chimp              # run from source
dotnet test                             # run all tests
dotnet publish -c Release -o out        # framework-dependent binary
dotnet publish -c Release -o out --self-contained  # standalone binary
```

Requires .NET SDK 10. Set `CHIMPCLI_DEBUG=1` for verbose API logging.

## Architecture

The code follows a strict layered design enforced by NsDepCop (config in `Chimp/NsDepCop.config`):

```
Shell  →  Services  →  Api  →  DomainModels
                    ↘  Common  ↙
```

- **`Shell/`** — CLI entry points: `CommandParser` maps raw args to `IShellCommand` implementations (one file per command). `RenderHelper` formats console output. `Localization.cs` holds all user-facing strings in English/Dutch.
- **`Services/TimeSheetService.cs`** — the single business-logic orchestrator; all commands delegate to it.
- **`Api/Client.cs`** — TimeChimp REST API client with in-process caching; `Api/AwsCognito/` handles AWS Cognito auth including MFA (software tokens).
- **`DomainModels/`** — immutable records: `TimeSheet` (week container), `TimeSheetRow` (single entry), `TimeEntry` (parses flexible time syntax), `ShortId` (type-safe display IDs).
- **`Common/`** — `PersistablePropertyBag` (JSON + DPAPI on Windows / chmod 0600 on Unix for `~/.chimpcli`), `Error` (localised exceptions), `DebugLogger`.
- **`Program.cs`** — wires everything together; no DI framework used.

## Key domain concepts

**Time entry syntax** (`TimeEntry.cs`) supports many forms:
- Duration: `1h30m`, `90m`
- Range: `9-10`, `09:00-10:30`, `930-1045`
- Start + duration: `9+30`, `14.30+90`
- Weekday prefix: `mo:9-10`, `vr:1h` (localized; `ma:` = Dutch Monday, etc.)

**Project/tag addressing:** users reference projects as `pN` and tags as `pN-A,B` (e.g. `p2-1,3`). The numbers are assigned at display time from the cached project list, so they can shift between sessions.

**Time travel:** a persisted offset lets users view/edit past weeks. Any active offset shows a warning in the UI.

## Testing

Tests use NUnit 4.5.1, Moq, Shouldly, and Bogus/AutoBogus. Run a single test file:

```bash
dotnet test --filter "FullyQualifiedName~TimeEntryTests"
```

Test structure mirrors `Chimp/` under `Chimp.Tests/`.
