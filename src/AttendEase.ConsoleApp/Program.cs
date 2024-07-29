using AttendEase.ConsoleApp.ViewModels;
using AttendEase.Shared.Enums;
using AttendEase.Shared.Services;
using Microsoft.Extensions.Configuration;
// Retrieve App Secrets
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var currentTime = DateTime.Now;
var fmNetViewModel = new FmNetViewModel();
var digiSheetViewModel = new DigiSheetViewModel();

#region Set Credentials from Json
string digiSheetUserName = config?.GetValue<string>("DigiSheet:UserName");
string digiSheetPassword = config?.GetValue<string>("DigiSheet:Password");
string fmNetUserName = config?.GetValue<string>("FmNet:UserName");
string fmNetPassword = config?.GetValue<string>("FmNet:Password");
digiSheetViewModel.Username = digiSheetUserName ?? "";
digiSheetViewModel.Password = digiSheetPassword ?? "";
fmNetViewModel.Username = fmNetUserName ?? "";
fmNetViewModel.Password = fmNetPassword ?? "";
#endregion

try
{
    fmNetViewModel.GetCredentials();
    await fmNetViewModel.Login();
    var fmNetAttendanceRecords = await fmNetViewModel.GetAttendanceRecords(DateOnly.FromDateTime(currentTime));
    await fmNetViewModel.SubmitAttendance();
    fmNetViewModel.Dispose();

    digiSheetViewModel.GetCredentials();
    await digiSheetViewModel.Login();
    var digiSheetPendingSubmitRecords = await digiSheetViewModel.GetAttendanceRecords(DateOnly.FromDateTime(currentTime));
    await digiSheetViewModel.SubmitAttendance(fmNetAttendanceRecords);

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    fmNetViewModel.Dispose();
    digiSheetViewModel.Dispose();
    Console.WriteLine("Press any key to exit");
    Console.ReadLine();
}