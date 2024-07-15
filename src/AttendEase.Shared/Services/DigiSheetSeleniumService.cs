using AttendEase.Shared.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AttendEase.Shared.Services;
public class DigiSheetSeleniumService : IDigiSheetService
{
    private IWebDriver _driver;
    public DigiSheetSeleniumService()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless"); // Run in headless mode

        // Configure ChromeDriverService to run without a window
        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(service, options);
    }

    public Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task Login(string staffingAgencyCd,string username, string password)
    {
        //url: randstad/staffLogin
        //form action="./d/d" method="POST" name="StaffLogin" onsubmit="return command( document.StaffLogin )"
        //input name HC,UI,Pw
        //button name loginButton
        throw new NotImplementedException();
    }

    public Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        throw new NotImplementedException();
    }
}