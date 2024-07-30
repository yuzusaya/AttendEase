using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AttendEase.Shared.Services;

public class FmNetSeleniumService : IFmNetService
{
    private IWebDriver _driver;
    private List<string> GetAttendanceRowsFromAttendanceTableHtml(string attendanceTableHtml)
    {
        //inside tbody, get all tr which td does not contains classname mg_header
        List<string> attendanceRows = new List<string>();

        // Load HTML document using HtmlAgilityPack
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(attendanceTableHtml);

        // Select all <tr> elements inside <tbody>, excluding those with <td> containing 'mg_header' or 'mg_sum' class
        var rows = doc.DocumentNode.SelectNodes("//tbody//tr[not(.//td[contains(@class, 'mg_header') or contains(@class, 'mg_sum')])]");

        if (rows != null)
        {
            foreach (var row in rows)
            {
                attendanceRows.Add(row.OuterHtml); // Add the outer HTML of each matching <tr> to the list
            }
        }

        return attendanceRows;
    }

    private AttendanceRecord ConvertToAttendanceRecordFromAttendanceRowHtml(string attendanceRowHtml)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(attendanceRowHtml);

        // Initialize record fields
        var record = new AttendanceRecord();

        // Select all <td> elements inside the row
        var tds = doc.DocumentNode.SelectNodes("//td");
        // var tds = doc.DocumentNode.SelectNodes("//td[@class='mg_dc_normal']");
        //either mg_dc_confirmed or mg_dc_normal

        if (tds != null)
        {
            // Extract date and time
            var dateNode = tds[1].SelectSingleNode(".//span[@id='DAY']");
            var monthNode = tds[1].SelectSingleNode(".//span[@id='MONTH']");
            if (dateNode != null && monthNode != null)
            {
                int day = int.Parse(dateNode.InnerText);
                int month = int.Parse(monthNode.InnerText);
                record.Date = new DateOnly(DateTime.Now.Year, month, day);
            }

            //todo the format will be diff if not yet approved, maybe check the status first
            // Extract start and end times
            var actualTimeSpans = tds[4].SelectNodes(".//span");
            if (actualTimeSpans?.Count >= 6)
            {
                //2nd span
                var actualStartTimeNode = actualTimeSpans[1];
                //3rd span
                var actualStartMinuteNode = actualTimeSpans[2];
                //5th span
                var actualEndTimeNode = actualTimeSpans[4];
                //6th span
                var actualEndMinuteNode = actualTimeSpans[5];
                if (actualStartTimeNode != null && actualStartMinuteNode != null && actualEndTimeNode != null && actualEndMinuteNode != null)
                {
                    int startHour = int.Parse(actualStartTimeNode.InnerText);
                    int startMinute = int.Parse(actualStartMinuteNode.InnerText);
                    int endHour = int.Parse(actualEndTimeNode.InnerText);
                    int endMinute = int.Parse(actualEndMinuteNode.InnerText);
                    record.ActualStartTime = new TimeOnly(startHour, startMinute);
                    record.ActualEndTime = new TimeOnly(endHour, endMinute);
                }
            }

            var timeSpans = tds[5].SelectNodes(".//span");
            if (timeSpans?.Count >= 6)
            {
                //2nd span
                var startTimeNode = timeSpans[1];
                //3rd span
                var startMinuteNode = timeSpans[2];
                //5th span
                var endTimeNode = timeSpans[4];
                //6th span
                var endMinuteNode = timeSpans[5];
                if (startTimeNode != null && startMinuteNode != null && endTimeNode != null && endMinuteNode != null)
                {
                    int startHour = int.Parse(startTimeNode.InnerText);
                    int startMinute = int.Parse(startMinuteNode.InnerText);
                    int endHour = int.Parse(endTimeNode.InnerText);
                    int endMinute = int.Parse(endMinuteNode.InnerText);
                    record.StartTime = new TimeOnly(startHour, startMinute);
                    record.EndTime = new TimeOnly(endHour, endMinute);
                }
            }
            // Extract status
            var statusNode = tds[10].InnerText.Trim();
            record.Status = statusNode switch
            {
                "入力済" => AttendanceStatus.Submitted,
                "確認済" => AttendanceStatus.Approved,
                _ => AttendanceStatus.NotYetFilled
            };

            record.Remarks = tds[20].InnerText.Replace("&nbsp;", "").Trim();
        }

        return record;
    }
    private void InitializeDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new"); // Run in headless mode

        // Configure ChromeDriverService to run without a window
        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(service, options);
    }
    public async Task<List<AttendanceRecord>> GetAttandanceRecords(DateOnly date)
    {
        //prev button id : TOPRVTM
        //next button id : TONXTTM
        //就労管理
        var laborManagementAnchor = _driver.FindElement(By.XPath($"//a[contains(text(), '就労管理')]"));
        var laborManagementLink = laborManagementAnchor.GetAttribute("href");
        await _driver.GoToUrl(laborManagementLink, By.XPath("//a[contains(text(), '勤務実績入力')]"));
        //勤務実績入力
        var workInputAnchor = _driver.FindElement(By.XPath($"//a[contains(text(), '勤務実績入力')]"));
        var workInputLink = workInputAnchor.GetAttribute("href");
        //input form
        await _driver.GoToUrl(workInputLink, By.Name("APPROVALGRD"));
        //get table
        var attendanceTable = _driver.FindElement(By.Name("APPROVALGRD"));
        var attendanceTableHtml = attendanceTable.GetAttribute("innerHTML");
        var attendanceRows = GetAttendanceRowsFromAttendanceTableHtml(attendanceTableHtml);
        List<AttendanceRecord> records = new();
        foreach (var row in attendanceRows)
        {
            var record = ConvertToAttendanceRecordFromAttendanceRowHtml(row);
            if (record.Date > DateOnly.FromDateTime(DateTime.Today))
            {
                break;
            }
            records.Add(record);
            Console.WriteLine(record);
        }
        return records;
    }

    public async Task Login(string username, string password)
    {
        InitializeDriver();
        // Login to FMNet
        await _driver.GoToUrl($"{Constants.FmNetBaseUrl}cws/cws", By.Id("login"));
        var uidField = _driver.FindElement(By.Name("uid"));
        uidField.SendKeys(username);
        var pwdField = _driver.FindElement(By.Name("pwd"));
        pwdField.SendKeys(password);
        var loginButton = _driver.FindElement(By.Name("Login"));
        loginButton.Click();

        //check if login success by checking if logout div is present
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Id("logout")));
    }

    public async Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks = "")
    {
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("APPROVALGRD")));
        //K2024_7_220STH
        //K2024_7_220STM
        //K2024_7_220ETH
        //K2024_7_220ETM
        //BTNDCDS2024_7_220
        var inputNameFormat = date.ToString("yyyy_M_d");
        var startHourInputName = $"K{inputNameFormat}0STH";
        var startMinuteInputName = $"K{inputNameFormat}0STM";
        var endHourInputName = $"K{inputNameFormat}0ETH";
        var endMinuteInputName = $"K{inputNameFormat}0ETM";
        var submitButtonName = $"BTNDCDS{inputNameFormat}0";
        var startHourInput = _driver.FindElement(By.Id(startHourInputName));
        var startMinuteInput = _driver.FindElement(By.Id(startMinuteInputName));
        var endHourInput = _driver.FindElement(By.Id(endHourInputName));
        var endMinuteInput = _driver.FindElement(By.Id(endMinuteInputName));
        var submitButton = _driver.FindElement(By.Id(submitButtonName));

        //fill the form
        var selectElement = new SelectElement(startHourInput);
        selectElement.SelectByValue(startTime.Hour.ToString());
        selectElement = new SelectElement(startMinuteInput);
        selectElement.SelectByValue(startTime.Minute.ToString());
        selectElement = new SelectElement(endHourInput);
        selectElement.SelectByValue(endTime.Hour.ToString());
        selectElement = new SelectElement(endMinuteInput);
        selectElement.SelectByValue(endTime.Minute.ToString());
        submitButton.Click();

        var confirmButtonElement = By.Id("dSubmission1");
        wait.Until(d => d.FindElement(confirmButtonElement));
        var confirmButton = _driver.FindElement(confirmButtonElement);
        confirmButton.Click();
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
