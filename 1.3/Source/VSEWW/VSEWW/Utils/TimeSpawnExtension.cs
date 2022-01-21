using System;

namespace VSEWW
{
    public static class TimeSpanExtension
    {
        public static string Verbose(this TimeSpan timeSpan)
        {
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (hours > 0) return string.Format("{0}:{1}:{2}", hours, minutes, seconds);
            return string.Format("{0}:{1}", minutes, seconds);
        }
    }
}
