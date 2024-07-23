using AttendEase.Shared.Enums;
using AttendEase.Shared.Services;

Console.WriteLine("Enter your username:");
var username = Console.ReadLine();
Console.WriteLine("Enter your password:");
var pass = string.Empty;
ConsoleKey key;
do
{
    var keyInfo = Console.ReadKey(intercept: true);
    key = keyInfo.Key;

    if (key == ConsoleKey.Backspace && pass.Length > 0)
    {
        Console.Write("\b \b");
        pass = pass[0..^1];
    }
    else if (!char.IsControl(keyInfo.KeyChar))
    {
        Console.Write("*");
        pass += keyInfo.KeyChar;
    }
} while (key != ConsoleKey.Enter);
Console.WriteLine("");
try
{
    var currentTime = DateTime.Now;
    IFmNetService fmNetService = new FmNetSeleniumService();
    await fmNetService.Login(username, pass);
    Console.WriteLine("Login successful");
    var attendanceRecords = await fmNetService.GetAttandanceRecords(DateOnly.FromDateTime(currentTime));
    var pendingSubmitRecords = attendanceRecords.Where(x => x.Status == AttendanceStatus.NotYetFilled && x.Date < DateOnly.FromDateTime(currentTime));
    var readyToSubmitRecords = pendingSubmitRecords.Where(x => x.Day == DayOfWeek.Saturday || x.Day == DayOfWeek.Sunday || x.SuggestedEndTime != default);
    Console.WriteLine($"{readyToSubmitRecords.Count()}/{pendingSubmitRecords.Count()} ready to submit");
    foreach (var record in readyToSubmitRecords)
    {
        await fmNetService.SubmitAttendance(record.Date, record.SuggestedStartTime, record.SuggestedEndTime);
    }
    // IDigiSheetService digiSheetService = new DigiSheetSeleniumService();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}