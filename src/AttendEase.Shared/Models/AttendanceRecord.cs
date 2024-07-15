using AttendEase.Shared.Enums;

namespace AttendEase.Shared.Models;
public record AttendanceRecord
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Remarks { get; set; }
    public AttendanceStatus Status { get; set; }
}