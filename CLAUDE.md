# TrackRepair - notes for Claude Code

A mod for Railroader's built-in mod loader (Unity/Mono, game 0.6.0; entry point `ModEntryPoint` in Main.cs, asset-pack Info.json). Adds the `/repairtrack [condition]` console command, which sets every completed track segment to a condition through `TrackConditionController.SetCondition`. Namespace `TrackRepair`, author Exp232.

## Build
- `dotnet build -c Release /p:DeployToGame=false` compiles against the game's DLLs (path set by `RailroaderDir` in the csproj).
- Do not deploy into the game's Mods folder unless asked.
- Your usual job here: build, read compile errors, fix them minimally without changing behaviour, repeat until clean, then show the diff.

## Conventions
- The game's console only scans Railroader.dll for `[ConsoleCommandHandler]` classes; `CommandRegistration.cs` adds this assembly to the console's private `_processor`. Command methods must be non-public.
- When changing behaviour, bump the version in `Info.json` and the csproj (`ReleaseVersion`) together and add a `CHANGELOG.md` entry (newest first).

## Workflow
New versions arrive as `TrackRepair-<ver>-source.zip` from another chat; the BuildWatcher (Desktop\Projects\BuildWatcher) imports, builds, commits, pushes and deploys them to Documents\Railroader\ModDev.
