using System.Collections.Generic;
using System.Linq;

namespace FrameShift
{
    /// <summary>
    /// The four user-configurable FPS modes (edited in Desktop mode) plus OFF.
    /// Keep MinFps/MaxFps in sync with RTSSBridge Program.IsValidFps.
    /// </summary>
    internal static class FpsModeRules
    {
        public const int ModeCount = 4;
        public const int MinFps = 10;
        public const int MaxFps = 500;
        public static readonly int[] Defaults = { 30, 40, 60, 120 };

        /// <summary>0 (OFF) or a whole number in [MinFps, MaxFps].</summary>
        public static bool IsValidLimit(int fps)
        {
            return fps == 0 || (fps >= MinFps && fps <= MaxFps);
        }

        /// <summary>Four distinct valid limits, sorted; anything else falls back to the defaults.</summary>
        public static bool IsValidModeSet(IList<int> modes)
        {
            return modes != null && modes.Count == ModeCount &&
                   modes.All(m => m > 0 && IsValidLimit(m)) && modes.Distinct().Count() == ModeCount;
        }

        public static List<int> Normalize(IList<int> modes)
        {
            return IsValidModeSet(modes) ? modes.OrderBy(m => m).ToList() : Defaults.ToList();
        }

        /// <summary>
        /// Menu entries: the four modes, the game's current limit if it is not one of them (so the active
        /// limit stays visible after the modes were changed), then OFF (0).
        /// </summary>
        public static List<int> MenuOptions(IList<int> modes, int current)
        {
            var options = Normalize(modes);
            if (current > 0 && !options.Contains(current)) options.Add(current);
            options.Sort();
            options.Add(0);
            return options;
        }
    }
}
