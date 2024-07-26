using AttendEase.ConsoleApp.ViewModels;
using AttendEase.Shared.Enums;
using AttendEase.Shared.Services;

var currentTime = DateTime.Now;
var fmNetViewModel = new FmNetViewModel();
var digiSheetViewModel = new DigiSheetViewModel();

try
{
    fmNetViewModel.GetCredentials();
    await fmNetViewModel.Login();
    var fmNetAttendanceRecords = await fmNetViewModel.GetAttendanceRecords(DateOnly.FromDateTime(currentTime));
    await fmNetViewModel.SubmitAttendance();

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