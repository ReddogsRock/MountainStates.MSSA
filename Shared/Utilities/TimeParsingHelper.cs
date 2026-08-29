using System;

namespace MountainStates.MSSA.Module.MSSA_Results.Utilities
{
    // Shared by the CSV score import and the Results entry grid so both accept the same
    // input format: ':' or '.' as the minutes/seconds separator - '.' is easier to type
    // on a numeric keypad while scoring at the trial than reaching for a colon. A value
    // with no separator at all is treated as a bare number of minutes.
    //
    // Examples: "10:35" -> 10:35, "10.35" -> 10:35, ".35" -> 0:35, "10" -> 10:00.
    public static class TimeParsingHelper
    {
        public static TimeSpan? ParseMinutesSeconds(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            char separator = trimmed.Contains(':') ? ':' : (trimmed.Contains('.') ? '.' : '\0');

            if (separator != '\0')
            {
                var parts = trimmed.Split(separator);
                if (parts.Length == 2)
                {
                    var minutesPart = string.IsNullOrEmpty(parts[0]) ? "0" : parts[0];
                    if (int.TryParse(minutesPart, out var minutes) && int.TryParse(parts[1], out var seconds))
                    {
                        return new TimeSpan(0, 0, minutes, seconds);
                    }
                }
                return null;
            }

            // No separator at all - bare number of minutes.
            if (int.TryParse(trimmed, out var minutesOnly))
            {
                return TimeSpan.FromMinutes(minutesOnly);
            }

            return null;
        }

        // Formats for round-trip back into an editable field, e.g. "10:35".
        public static string Format(TimeSpan? ts)
        {
            if (!ts.HasValue) return "";
            int totalMinutes = (int)ts.Value.TotalMinutes;
            int seconds = ts.Value.Seconds;
            return $"{totalMinutes}:{seconds:D2}";
        }
    }
}
