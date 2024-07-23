using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendEase.Shared.Models;

namespace AttendEase.Shared.Services;
public class FmNetApiService : IFmNetService
{
    public Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task Login(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        throw new NotImplementedException();
    }
}