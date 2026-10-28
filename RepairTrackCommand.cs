using System.Globalization;
using System.Linq;
using Game;
using Game.Mow;
using Game.State;
using Track;
using UI.Console;

namespace TrackRepair
{
    /// <summary>
    /// /repairtrack [condition]  - sets the condition of every completed track segment to the given value
    /// (default 100%). Segments already at or above it are left alone. Uses the game's own
    /// TrackConditionController.SetCondition, so posted track classes and speed limits update as they do when
    /// the debug window sets a condition.
    /// </summary>
    [ConsoleCommandHandler("/repairtrack", "Repair all track to 100% condition (or /repairtrack 80%)")]
    public class RepairTrackCommand
    {
        [ConsoleDefaultCommand]
        private static string Execute(string condition = "100%")
        {
            if (!StateManager.IsHost)
                throw new ConsoleCommandError("Host only.");

            float target = ParseCondition(condition);
            var controller = TrackConditionController.Shared;
            var graph = Graph.Shared;
            if (controller == null || graph == null)
                throw new ConsoleCommandError("Track condition is not available yet (is a game loaded?).");

            var now = TimeWeather.Now;
            int repaired = 0, total = 0;
            float before = 0f;

            // Copy first: SetCondition raises change events that may touch the graph's collections.
            foreach (TrackSegment segment in graph.AllSegments.ToList())
            {
                if (!segment.IsRightOfWay || segment.ConstructionState != TrackConstructionState.Complete)
                    continue;
                total++;
                float current = controller.GetCondition(segment);
                if (current >= target - 0.0005f) continue;
                before += current;
                controller.SetCondition(segment, target, now);
                repaired++;
            }

            if (repaired == 0)
                return $"All {total} track segments are already at {target * 100f:F0}% or better.";
            return $"Repaired {repaired} of {total} track segments to {target * 100f:F0}% (average was {before / repaired * 100f:F0}%).";
        }

        private static float ParseCondition(string text)
        {
            string s = (text ?? "100%").Trim().TrimEnd('%');
            if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                throw new ConsoleCommandError("Condition must be a number, e.g. /repairtrack 100% or /repairtrack 80.");
            // "0.8" means 80%, "80" or "80%" also mean 80%.
            if (value > 1f || text.Contains("%")) value /= 100f;
            if (value <= 0f || value > 1f)
                throw new ConsoleCommandError("Condition must be between 1% and 100%.");
            return value;
        }
    }
}
