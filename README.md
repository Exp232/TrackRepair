# Track Repair

Console commands for Railroader 0.6.0+ (host only; single player is always host):

- `/repairtrack` - set every completed track segment to 100% condition.
- `/repairtrack 80%` - raise segments below 80% condition up to 80% (better track is untouched).
- `/settrackwear 20%` - set every completed track segment to 20% wear (= 80% condition), better or worse than it is now.
- `/settrackwear` - same as 0% wear (brand new track).

Wear is the opposite of condition: 0% wear = 100% condition. The game's track classes follow condition: above 80% is class 5
(80 mph limit), above 60% class 4 (60), above 40% class 3 (40), above 20% class 2 (25), otherwise class 1 (10).

Both use the game's own track condition controller, so posted track classes and speed limits update the same way they
do from the debug window. Requires "Allow Unsafe DLL Mods".

Build: `dotnet build -c Release /p:DeployToGame=false` (set `/p:RailroaderDir=...` if the game is elsewhere).
