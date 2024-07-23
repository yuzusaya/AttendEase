using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendEase.Shared.Utilities;
public static class TimeUtility
{
    public const int MinDiff = 8;
    public const int MaxDiff = 15;

    private static bool WithinRange(TimeOnly time1, TimeOnly time2)
    {
        var diff = time1 - time2;
        var minutesDiff = double.Abs(diff.TotalMinutes);
        return minutesDiff >= MinDiff && minutesDiff <= MaxDiff;
    }

    public static TimeOnly GetCheckInTimeFromStartTime(TimeOnly actualTime)
    {
        if (actualTime == default)
        {
            return actualTime;
        }
        // Calculate the check-in time by adding 10 minutes to actualTime
        TimeSpan addedTime = TimeSpan.FromMinutes(10);
        TimeOnly checkInTime = actualTime.Add(addedTime);

        // Round checkInTime to the nearest 5-minute interval
        int minutes = checkInTime.Minute;
        int roundedMinutes = ((minutes + 4) / 5) * 5; // Round up to the nearest 5-minute interval
        checkInTime = new TimeOnly(checkInTime.Hour, 0).AddMinutes(roundedMinutes);
        var previousCheckInTime = checkInTime.AddMinutes(-5);
        var nextCheckInTime = checkInTime.AddMinutes(5);
        var checkInTimeList = new List<TimeOnly>()
        {
            previousCheckInTime,checkInTime,nextCheckInTime
        };

        return checkInTimeList.Where(x => WithinRange(x, actualTime)).Min();
    }

    public static TimeOnly GetCheckOutTimeFromEndTime(TimeOnly actualTime)
    {
        if (actualTime == default)
        {
            return actualTime;
        }
        // Calculate the check-out time by subtracting 10 minutes from actualTime
        TimeSpan subtractedTime = TimeSpan.FromMinutes(10);
        TimeOnly checkOutTime = actualTime.Add(-subtractedTime);

        // Round checkOutTime to the nearest 5-minute interval
        int minutes = checkOutTime.Minute;
        int roundedMinutes = (minutes / 5) * 5; // Round down to the nearest 5-minute interval
        checkOutTime = new TimeOnly(checkOutTime.Hour, 0).AddMinutes(roundedMinutes);
        var previousCheckOutTime = checkOutTime.AddMinutes(-5);
        var nextCheckOutTime = checkOutTime.AddMinutes(5);
        var checkOutTimeList = new List<TimeOnly>()
        {
            previousCheckOutTime,checkOutTime,nextCheckOutTime
        };

        return checkOutTimeList.Where(x => WithinRange(actualTime, x)).Max();
    }
}
