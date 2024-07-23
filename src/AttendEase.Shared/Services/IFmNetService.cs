using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendEase.Shared.Models;

namespace AttendEase.Shared.Services;
public interface IFmNetService
{
    Task Login(string username, string password);
    Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date);
    Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks);
}