using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AttendEase.Shared.Services;
public static class SeleniumExtensionMethods
{
    public static async Task GoToUrl(this IWebDriver driver, string url, By by)
    {
        int attempts = 0;
        bool isSuccessful = false;

        while (attempts < Constants.MaxRetries && !isSuccessful)
        {
            try
            {
                driver.Navigate().GoToUrl(Constants.FmNetBaseUrl + url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
                wait.Until(d => d.FindElement(by));

                isSuccessful = true;
            }
            catch (WebDriverTimeoutException)
            {
                attempts++;
                if (attempts >= Constants.MaxRetries)
                {
                    throw;
                }
                await Task.Delay(Constants.RetryDelay);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while navigating to the {url}", ex);
            }
        }
    }
}