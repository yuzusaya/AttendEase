using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AttendEase.Shared.Services;
public class DigiSheetSeleniumService : IDigiSheetService
{
    private IWebDriver _driver;
    private void InitializeDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new"); // Run in headless mode

        // Configure ChromeDriverService to run without a window
        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(service, options);
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecords(DateOnly date)
    {
        // var previousBtn = _driver.FindElement(By.XPath("//input[@value='＜＜']"));
        // var nextBtn = _driver.FindElement(By.XPath("//input[@value='＞＞']"));
        var records = new List<AttendanceRecord>();
        var attendanceReportDiv = _driver.FindElement(By.XPath($"//div[contains(text(), '勤務報告')]"));
        var attendanceReportAnchor = attendanceReportDiv.FindElement(By.XPath(".."));
        attendanceReportAnchor.Click();
        _driver.SwitchTo().DefaultContent();
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("main")));
        _driver.SwitchTo().Frame("main");
        //get all tr which contains input with name starts with DayTable_WorkDate
        var inputElements = _driver.FindElements(By.XPath("//input[starts-with(@name, 'DayTable_WorkDate')]"));
        foreach (var inputElement in inputElements)
        {
            //get parent of parent
            var trElement = inputElement.FindElement(By.XPath("../.."));
            var record = ConvertToAttendanceRecordFromAttendanceRowHtml(trElement.GetAttribute("innerHTML"));
            if (record.Date > DateOnly.FromDateTime(DateTime.Today))
            {
                break;
            }
            records.Add(record);
            Console.WriteLine(record);
        }
        return records;
    }
    private AttendanceRecord ConvertToAttendanceRecordFromAttendanceRowHtml(string attendanceRowHtml)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(attendanceRowHtml);

        // Initialize record fields
        var record = new AttendanceRecord();

        // Select all <td> elements inside the row
        var tds = doc.DocumentNode.SelectNodes("//td");

        if (tds != null)
        {
            //../Image/shionin.gif
            var shioninImage = tds[0].SelectSingleNode("//img");
#warning need to check html of other status
            if (shioninImage != null)
            {
                record.Status = shioninImage.GetAttributeValue("src", "").Contains("/shionin.gif") ? AttendanceStatus.Submitted : AttendanceStatus.Approved;
            }
            else
            {
                record.Status = AttendanceStatus.NotYetFilled;
            }
            //determine the status based on the href of the img in 1st node
            // Extract date and time from 1st node
            var dateNode = tds[0].SelectSingleNode(".//input");
            if (dateNode != null)
            {
                var dateValue = dateNode.GetAttributeValue("value", "");
                if (DateTime.TryParse(dateValue, out var date))
                {
                    record.Date = DateOnly.FromDateTime(date);
                }
            }
            //extract start time from 5th node
            var startTimeNode = tds[4].SelectSingleNode(".//font");
            if (startTimeNode != null)
            {
                var startTimeValue = startTimeNode.InnerText;
                if (TimeOnly.TryParse(startTimeValue, out var startTime))
                {
                    record.StartTime = startTime;
                }
            }
            //extract end time from 6th node
            var endTimeNode = tds[5].SelectSingleNode(".//font");
            if (endTimeNode != null)
            {
                var endTimeValue = endTimeNode.InnerText;
                if (TimeOnly.TryParse(endTimeValue, out var endTime))
                {
                    record.EndTime = endTime;
                }
            }
            // extract rest hour from 7th node
            // var restHourNode = tds[6].SelectSingleNode(".//font");
            // if (restHourNode != null)
            // {
            //     var restHourValue = restHourNode.InnerText;
            //     if (TimeOnly.TryParse(restHourValue, out var restHour))
            //     {

            //     }
            // }

            // Extract remarks from 13th node
            var remarksNode = tds[12].SelectSingleNode("div/font");
            if (remarksNode != null)
            {
                record.Remarks = remarksNode.InnerText.Replace("&nbsp;", "").Replace("\r\n", "").Trim();
            }
        }
        return record;
    }
    public async Task Login(string staffingAgencyCd, string username, string password)
    {
        InitializeDriver();
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
#warning フレーム対応のブラウザでもう一度アクセスしてください

        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("menu")));
        _driver.SwitchTo().Frame("menu");
    }

    public async Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        // WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        // wait.Until(d => d.FindElement(By.Name("StaffWorkSheet")));
        //<input type="hidden" name="DayTable_WorkDate_0" value="2024-07-16">
        var inputElements = _driver.FindElements(By.XPath("//input[starts-with(@name, 'DayTable_WorkDate')]"));
        var dateNameFormat = date.ToString("yyyy-MM-dd");
        var dateFound = false;
        foreach (var inputElement in inputElements)
        {
            if (inputElement.GetAttribute("value") == dateNameFormat)
            {
                var trElement = inputElement.FindElement(By.XPath("../.."));
                //3rd td of trElement
                var anchorTd = trElement.FindElement(By.XPath("td[3]"));
                var anchor = anchorTd.FindElement(By.TagName("a"));
                anchor.Click();
                dateFound = true;
                await KeyInDetail(startTime, endTime, remarks);
                SubmitChanges();
                break;
            }
        }
        if (!dateFound)
        {
            throw new Exception($"Date {dateNameFormat} not found in the attendance report");
        }
    }
    private void SubmitChanges()
    {
        //find all checkbox
        var checkBoxes = _driver.FindElements(By.XPath("//input[@type='checkbox']"));
        foreach (var checkBox in checkBoxes)
        {
            checkBox.Click();
        }
        //find input with value update
        var updateButton = _driver.FindElement(By.XPath("//input[@value='更　新']"));
        updateButton?.Click();
    }

    private async Task KeyInDetail(TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Constants.PageTimeoutSeconds));
        wait.Until(d => d.FindElement(By.Name("RegistButton")));
        var hourStartInput = _driver.FindElement(By.Name("HourStart"));//drop down
        var selectElement = new SelectElement(hourStartInput);
        selectElement.SelectByValue(startTime.Hour.ToString());

        var minuteStartInput = _driver.FindElement(By.Name("MinuteStart"));
        minuteStartInput.Clear();
        minuteStartInput.SendKeys(startTime.Minute.ToString());

        var hourEndInput = _driver.FindElement(By.Name("HourEnd"));//drop down
        selectElement = new SelectElement(hourEndInput);
        selectElement.SelectByValue(endTime.Hour.ToString());

        var minuteEndInput = _driver.FindElement(By.Name("MinuteEnd"));
        minuteEndInput.Clear();
        minuteEndInput.SendKeys(endTime.Minute.ToString());

        //find input with name restAdjust(if exists,click it)
        try
        {
            var restAdjustInput = _driver.FindElement(By.Name("restAdjust"));
            if (restAdjustInput != null)
            {
                restAdjustInput.Click();
            }
        }
        catch (Exception)
        {
            //ignore
        }
        var commentInput = _driver.FindElement(By.Name("CommentInput"));
        commentInput.Clear();
        commentInput.SendKeys(remarks);

        var hourRestInput = _driver.FindElement(By.Name("HourRest"));//drop down
        selectElement = new SelectElement(hourRestInput);
        selectElement.SelectByValue("1");

        var registerButton = _driver.FindElement(By.Name("RegistButton"));
        registerButton.Click();
    }
}