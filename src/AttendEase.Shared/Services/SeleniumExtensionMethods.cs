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
                Console.WriteLine($"Navigating to {url}");
                driver.Navigate().GoToUrl(url);

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
                Console.WriteLine($"An error occurred while navigating to the {url}");
                throw;
            }
        }
    }
}