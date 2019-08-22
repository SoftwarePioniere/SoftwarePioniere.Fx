using System;
using System.Collections.Generic;

namespace SoftwarePioniere
{
    public static class TimeZoneExtensions
    {
        public static DateTime[] GetAllDays(this TimeZoneInfo tz, int untilDays = 365)
        {
            var list = new List<DateTime>();

            var jetzt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date, tz).Date;

            var d = jetzt;
            do
            {
                list.Add(d);
                d = d.AddDays(1);

            } while (d <= jetzt.AddDays(untilDays));

            return list.ToArray();
        }

        public static DateTime ConvertTimeFromUtc(this TimeZoneInfo tz, DateTime dateTimeUtc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        }
    }
}
