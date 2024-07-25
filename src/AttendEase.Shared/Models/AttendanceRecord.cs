using AttendEase.Shared.Enums;
using AttendEase.Shared.Utilities;
using System.Text;

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
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{Date.ToString("dd/MM")}({Day.ToString()}) - {Status}:");
        sb.AppendLine($"Time: {StartTime.ToString()} - {EndTime.ToString()}");
        sb.AppendLine($"Actual Time: {ActualStartTime.ToString()} - {ActualEndTime.ToString()}");
        if (!string.IsNullOrWhiteSpace(Remarks))
        {
            sb.AppendLine($"Remarks: {Remarks}");
        }
        return sb.ToString();
    }
    public string ToString(int index)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{Date.ToString("dd/MM")}({Day.ToString()}):");
        sb.AppendLine($"Time: {StartTime.ToString()} - {EndTime.ToString()}");
        sb.AppendLine($"Actual Time that will be key in: {SuggestedStartTime.ToString()} - {SuggestedEndTime.ToString()}");
        if (!string.IsNullOrWhiteSpace(Remarks))
        {
            sb.AppendLine($"Remarks: {Remarks}");
        }
        return sb.ToString();
    }
}