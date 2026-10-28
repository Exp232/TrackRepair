# Track Repair

Adds one console command for Railroader 0.6.0+:

- `/repairtrack` - set every completed track segment to 100% condition.
- `/repairtrack 80%` - set segments below 80% up to 80% (already-better track is untouched).

Host only (single player is always host). Uses the game's own track condition controller, so posted track classes and
speed limits update the same way they do from the debug window. Requires "Allow Unsafe DLL Mods".

Build: `dotnet build -c Release /p:DeployToGame=false` (set `/p:RailroaderDir=...` if the game is elsewhere).
