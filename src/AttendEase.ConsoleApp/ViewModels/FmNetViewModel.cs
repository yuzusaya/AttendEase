using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using AttendEase.Shared.Services;

namespace AttendEase.ConsoleApp.ViewModels;

public class FmNetViewModel
{
    private readonly IFmNetService _fmNetService;

    public string Username { get; set; }
    public string Password { get; set; }
    public List<AttendanceRecord> Records { get; private set; }

    public void GetCredentials()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            Console.WriteLine("Enter your username (FM-Net):");
            Username = Console.ReadLine();
        }
        if (string.IsNullOrWhiteSpace(Password))
        {

            Console.WriteLine("Enter your password (FM-Net):");
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
    public FmNetViewModel()
    {
        //_fmNetService = new FmNetPlaywrightService();
        _fmNetService = new FmNetSeleniumService();
    }
    public async Task Login()
    {
        await _fmNetService.Login(Username, Password);
        Console.WriteLine("Login successfully to FM-net");
    }
    public async Task<List<AttendanceRecord>> GetAttendanceRecords(DateOnly date)
    {
        Records = await _fmNetService.GetAttendanceRecords(date);
        return Records;
    }
    public async Task SubmitAttendance()
    {
        var currentTime = DateTime.Now;
        var pendingSubmitRecords = Records.Where(x => x.Status == AttendanceStatus.NotYetFilled && x.Date < DateOnly.FromDateTime(currentTime));
        var readyToSubmitRecords = pendingSubmitRecords.Where(x => x.Day == DayOfWeek.Saturday || x.Day == DayOfWeek.Sunday || x.SuggestedEndTime != default);
        Console.WriteLine($"{readyToSubmitRecords.Count()}/{pendingSubmitRecords.Count()} ready to submit");
        foreach (var record in readyToSubmitRecords)
        {
            try
            {
                record.ActualStartTime = record.SuggestedStartTime;
                record.ActualEndTime = record.SuggestedEndTime;
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine(record.ToString(1));
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine("Submit to FM-Net? (Y/N)");
                var keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key != ConsoleKey.Y)
                {
                    continue;
                }
                await _fmNetService.SubmitAttendance(record.Date, record.ActualStartTime, record.ActualEndTime);
                Console.WriteLine($"Submitted {record.Date} to FM-Net");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public void Dispose()
    {
        Records?.Clear();
        _fmNetService.Dispose();
    }
}
