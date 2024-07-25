using AttendEase.Shared.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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

    public void Dispose()
    {
        _driver?.Dispose();
    }

    public Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date)
    {
        throw new NotImplementedException();
    }

    public async Task Login(string staffingAgencyCd, string username, string password)
    {
        //url: randstad/staffLogin
        //form action="./d/d" method="POST" name="StaffLogin" onsubmit="return command( document.StaffLogin )"
        //input name HC,UI,Pw
        //button name loginButton
        await _driver.GoToUrl($"{Constants.DigiSheetBaseUrl}randstad/staffLogin", By.Name("StaffLogin"));
        var staffingAgencyCdInput = _driver.FindElement(By.Name("HC"));
        staffingAgencyCdInput.SendKeys(Constants.DigiSheetStaffingAgencyCd);
        var userNameInput = _driver.FindElement(By.Name("UI"));
        userNameInput.SendKeys(username);
        var passwordInput = _driver.FindElement(By.Name("Pw"));
        passwordInput.SendKeys(password);
        var loginButton = _driver.FindElement(By.Name("loginButton"));
        loginButton.Click();

        //check if login success by checking if logout div is present
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("MenuForm")));
    }

    public async Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        var attendanceReportAnchor = _driver.FindElement(By.XPath($"//a[contains(text(), '勤務報告')]"));
        attendanceReportAnchor.Click();
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("StaffWorkSheet")));
        
        throw new NotImplementedException();
    }
}