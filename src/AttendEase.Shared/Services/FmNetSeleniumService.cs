using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AttendEase.Shared.Services;

public class FmNetSeleniumService : IFmNetService
{
    private IWebDriver _driver;
    public FmNetSeleniumService()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless"); // Run in headless mode

        // Configure ChromeDriverService to run without a window
        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(service, options);
    }

    public async Task Login(string username, string password)
    {
        // Login to FMNet
        // await _driver.GoToUrl("cws/cws", By.Id("login"));
    }
}
