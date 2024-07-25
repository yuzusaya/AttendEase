using AttendEase.Shared.Models;

namespace AttendEase.Shared.Services;
public interface IDigiSheetService
{
    Task Login(string staffingAgencyCd, string username, string password);
    Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date);
    Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks);
    void Dispose();
}

