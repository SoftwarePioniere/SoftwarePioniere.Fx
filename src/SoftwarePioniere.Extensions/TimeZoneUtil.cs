using System;
using System.Linq;

namespace SoftwarePioniere
{
    public static class TimeZoneUtil
    {
        private static TimeZoneInfo _tz;

        public static string[] HomeZones { get; set; } = {
            "Europe/Berlin",
            "W. Europe Standard Time"
        };

        public static TimeZoneInfo GetHomeTimeZone()
        {

            if (_tz != null)
                return _tz;

            foreach (var zone in HomeZones)
            {
                try
                {
                    if (TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id == zone) != null)
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(zone);
                        _tz = tz;
                        return _tz;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TimeZone Error: {0} {1}", zone, ex.Message);
                }
            }


            _tz = TimeZoneInfo.Local;
            return _tz;

        }
    }
}
