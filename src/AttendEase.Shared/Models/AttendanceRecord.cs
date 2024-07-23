using AttendEase.Shared.Enums;
using AttendEase.Shared.Utilities;

namespace AttendEase.Shared.Models;
public record AttendanceRecord
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly ActualStartTime { get; set; }
    public TimeOnly ActualEndTime { get; set; }
    public string Remarks { get; set; }
    public AttendanceStatus Status { get; set; }
    public DayOfWeek Day => Date.ToDateTime(StartTime).DayOfWeek;
    public TimeOnly SuggestedStartTime => TimeUtility.GetCheckInTimeFromStartTime(StartTime);
    public TimeOnly SuggestedEndTime => TimeUtility.GetCheckOutTimeFromEndTime(EndTime);
}