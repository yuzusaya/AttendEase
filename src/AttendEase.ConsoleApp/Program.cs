using AttendEase.Shared.Enums;
using AttendEase.Shared.Services;

Console.WriteLine("Enter your username (FM-Net):");
var username = Console.ReadLine();
Console.WriteLine("Enter your password (FM-Net):");
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
    IDigiSheetService digiSheetService = new DigiSheetSeleniumService();
    await fmNetService.Login(username, pass);
    Console.WriteLine("Login successfully to FM-net");
    var attendanceRecords = await fmNetService.GetAttandanceRecords(DateOnly.FromDateTime(currentTime));
    var pendingSubmitRecords = attendanceRecords.Where(x => x.Status == AttendanceStatus.NotYetFilled && x.Date < DateOnly.FromDateTime(currentTime));
    var readyToSubmitRecords = pendingSubmitRecords.Where(x => x.Day == DayOfWeek.Saturday || x.Day == DayOfWeek.Sunday || x.SuggestedEndTime != default);
    Console.WriteLine($"{readyToSubmitRecords.Count()}/{pendingSubmitRecords.Count()} ready to submit");
    if (readyToSubmitRecords.Any())
    {
        //todo get username and password for digisheet
        //await digiSheetService.Login("5018", "", pass);
        foreach (var record in readyToSubmitRecords)
        {
            Console.WriteLine(record.ToString(1));
            Console.WriteLine("Submit to FM-Net? (Y/N)");
            var keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key != ConsoleKey.Y)
            {
                continue;
            }
            await fmNetService.SubmitAttendance(record.Date, record.SuggestedStartTime, record.SuggestedEndTime);
            Console.WriteLine($"Submitted {record.Date} to FM-Net");
            //if success, key in to digisheet as well (if related)
            //todo get remarks
            //await digiSheetService.SubmitAttendance(record.Date, record.SuggestedStartTime, record.SuggestedEndTime,"");
        }
    }
    Console.ReadLine();
    fmNetService.Dispose();
    digiSheetService.Dispose();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.ReadLine();
}