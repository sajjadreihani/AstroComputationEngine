using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Utility;

public static class DateTimeExtensions
{
    public static DateTime ToUtcFromTimeZone(this DateTime dateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var localTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
    }

    public static DateTime FromUtcToTimeZone(this DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }
}
