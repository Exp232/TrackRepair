using System;
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
    /// /repairtrack [condition]  - raises every completed track segment to the given condition (default 100%).
    /// Segments already at or above it are left alone. Uses the game's own TrackConditionController.SetCondition,
    /// so posted track classes and speed limits update as they do when the debug window sets a condition.
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
            var r = TrackWork.Apply(target, raiseOnly: true);
            if (r.Changed == 0)
                return $"All {r.Total} track segments are already at {target * 100f:F0}% or better.";
            return $"Repaired {r.Changed} of {r.Total} track segments to {target * 100f:F0}% (average was {r.AverageBefore * 100f:F0}%).";
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

    /// <summary>
    /// /setwear [percent]  - sets the wear of every completed track segment to the given percentage
    /// (0% = brand new, 100% = completely worn out; default 0%). Unlike /repairtrack this can also make track
    /// worse, which is useful for testing. Wear is the opposite of condition: 20% wear = 80% condition.
    /// </summary>
    [ConsoleCommandHandler("/setwear", "Set all track to a wear percentage: /setwear 20% (0% = new, 100% = worn out)")]
    public class SetWearCommand
    {
        [ConsoleDefaultCommand]
        private static string Execute(string wear = "0%")
        {
            if (!StateManager.IsHost)
                throw new ConsoleCommandError("Host only.");

            string s = (wear ?? "0%").Trim().TrimEnd('%');
            if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float percent))
                throw new ConsoleCommandError("Wear must be a number, e.g. /setwear 20% (0% = new, 100% = worn out).");
            if (percent < 0f || percent > 100f)
                throw new ConsoleCommandError("Wear must be between 0% and 100%.");

            float target = 1f - percent / 100f;
            var r = TrackWork.Apply(target, raiseOnly: false);
            if (r.Changed == 0)
                return $"All {r.Total} track segments are already at {percent:F0}% wear.";
            return $"Set {r.Changed} of {r.Total} track segments to {percent:F0}% wear ({target * 100f:F0}% condition, track class {ClassFor(target)}).";
        }

        // The game's class thresholds: condition > 80% = class 5, > 60% = 4, > 40% = 3, > 20% = 2, otherwise 1.
        private static int ClassFor(float condition)
        {
            if (condition > 0.8f) return 5;
            if (condition > 0.6f) return 4;
            if (condition > 0.4f) return 3;
            if (condition > 0.2f) return 2;
            return 1;
        }
    }

    internal static class TrackWork
    {
        internal struct Result
        {
            public int Total, Changed;
            public float AverageBefore;
        }

        /// <summary>
        /// Sets the condition (1 = new) of every completed right-of-way segment. With raiseOnly, track that is
        /// already better than the target is left alone.
        /// </summary>
        internal static Result Apply(float targetCondition, bool raiseOnly)
        {
            var controller = TrackConditionController.Shared;
            var graph = Graph.Shared;
            if (controller == null || graph == null)
                throw new ConsoleCommandError("Track condition is not available yet (is a game loaded?).");

            var now = TimeWeather.Now;
            var result = new Result();
            float before = 0f;

            // Copy first: SetCondition raises change events that may touch the graph's collections.
            foreach (TrackSegment segment in graph.AllSegments.ToList())
            {
                if (!segment.IsRightOfWay || segment.ConstructionState != TrackConstructionState.Complete)
                    continue;
                result.Total++;
                float current = controller.GetCondition(segment);
                bool already = raiseOnly
                    ? current >= targetCondition - 0.0005f
                    : Math.Abs(current - targetCondition) <= 0.0005f;
                if (already) continue;
                before += current;
                controller.SetCondition(segment, targetCondition, now);
                result.Changed++;
            }
            result.AverageBefore = result.Changed > 0 ? before / result.Changed : 0f;
            return result;
        }
    }
}
