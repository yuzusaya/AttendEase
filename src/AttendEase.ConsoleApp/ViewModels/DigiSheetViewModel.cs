namespace AttendEase.ConsoleApp.ViewModels;
using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using AttendEase.Shared.Services;

public class DigiSheetViewModel
{
    private readonly IDigiSheetService _digiSheetService;

    public DigiSheetViewModel()
    {
        _digiSheetService = new DigiSheetSeleniumService();
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public List<AttendanceRecord> Records { get; private set; }

    public void GetCredentials()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            Console.WriteLine("Enter your DigiSheet username (Press the enter key if not applicable):");
            Username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(Username))
            {
                throw new Exception("");
            }
            Console.WriteLine("Enter your DigiSheet password:");
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            Password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && Password.Length > 0)
                {
                    Console.Write("\b \b");
                    Password = Password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    Password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine("");
        }
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            throw new Exception("Username and password cannot be empty");
        }
    }

    public async Task Login()
    {
        await _digiSheetService.Login("5018", Username, Password);
        Console.WriteLine("Login successfully to DigiSheet");
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecords(DateOnly date)
    {
        Records = await _digiSheetService.GetAttandanceRecords(date);
        return Records;
    }

    public async Task SubmitAttendance(List<AttendanceRecord> fmNetAttendanceRecords)
    {
        var currentTime = DateTime.Now;
        var digiSheetPendingSubmitRecords = Records.Where(x => x.Status == AttendanceStatus.NotYetFilled && x.Date < DateOnly.FromDateTime(currentTime));
        foreach (var record in digiSheetPendingSubmitRecords)
        {
            try
            {
                var attendanceRecordInFmNet = fmNetAttendanceRecords.FirstOrDefault(x => x.Date == record.Date);
                if (attendanceRecordInFmNet == null)
                {
                    Console.WriteLine($"Attendance record for {record.Date} not found in FM-Net");
                    continue;
                }
                record.ActualStartTime = attendanceRecordInFmNet.SuggestedStartTime;
                record.ActualEndTime = attendanceRecordInFmNet.SuggestedEndTime;
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine(record.ToString(1));
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine("Submit to DigiSheet? (Y/N)");
                var keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key != ConsoleKey.Y)
                {
                    continue;
                }
                Console.WriteLine("Please write remarks (if any):");
                var remarks = Console.ReadLine();
                await _digiSheetService.SubmitAttendance(record.Date, record.ActualStartTime, record.ActualEndTime, remarks);
                Console.WriteLine($"Submitted {record.Date} to DigiSheet");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting attendance for {record.Date}: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        Records?.Clear();
        _digiSheetService.Dispose();
    }

}