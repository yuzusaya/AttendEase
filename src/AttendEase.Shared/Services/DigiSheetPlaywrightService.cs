using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendEase.Shared.Services;

public class DigiSheetPlaywrightService : IDigiSheetService
{
    private IFrameLocator _currentFrameLocator;
    private IBrowser _browser;
    private IPage _page;
    private PageWaitForSelectorOptions _pageWaitForSelectorOptions = new PageWaitForSelectorOptions
    {
        State = WaitForSelectorState.Attached,
    };

    private async Task InitializeDriverAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
            //Headless = false
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecords(DateOnly date)
    {
        var records = new List<AttendanceRecord>();
        _currentFrameLocator = _page.FrameLocator("frame[name=menu]");
        var attendanceReportDiv = await _currentFrameLocator.Locator("//div[contains(text(), '勤務報告')]").ElementHandleAsync();
        var attendanceReportAnchor = await attendanceReportDiv.QuerySelectorAsync("..");
        await attendanceReportAnchor.ClickAsync();

        await _page.WaitForSelectorAsync("frame[name=main]", _pageWaitForSelectorOptions);
        _currentFrameLocator = _page.FrameLocator("frame[name=main]");
        
        ILocator inputElements = null;
        int count = 0;
        var trial = 10;
        while (trial > 0)
        {
            inputElements = _currentFrameLocator.Locator("//input[starts-with(@name, 'DayTable_WorkDate')]");
            count = await inputElements.CountAsync();
            if (count > 0)
            {
                break;
            }
            await Task.Delay(200);
            trial--;
        }

        for (int i = 0; i < count; i++)
        {
            var inputElement = inputElements.Nth(i);
            var parentElement = await inputElement.Locator("xpath=../..").ElementHandleAsync();
            var record = ConvertToAttendanceRecordFromAttendanceRowHtml(await parentElement.InnerHTMLAsync());
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

        var record = new AttendanceRecord();
        var tds = doc.DocumentNode.SelectNodes("//td");

        if (tds != null)
        {
            var shioninImage = tds[0].SelectSingleNode("//img");
            if (shioninImage != null)
            {
                record.Status = shioninImage.GetAttributeValue("src", "").Contains("/shionin.gif")
                    ? AttendanceStatus.Submitted
                    : AttendanceStatus.Approved;
            }
            else
            {
                record.Status = AttendanceStatus.NotYetFilled;
            }

            var dateNode = tds[0].SelectSingleNode(".//input");
            if (dateNode != null)
            {
                var dateValue = dateNode.GetAttributeValue("value", "");
                if (DateTime.TryParse(dateValue, out var date))
                {
                    record.Date = DateOnly.FromDateTime(date);
                }
            }

            var startTimeNode = tds[4].SelectSingleNode(".//font");
            if (startTimeNode != null && TimeOnly.TryParse(startTimeNode.InnerText, out var startTime))
            {
                record.StartTime = startTime;
            }

            var endTimeNode = tds[5].SelectSingleNode(".//font");
            if (endTimeNode != null && TimeOnly.TryParse(endTimeNode.InnerText, out var endTime))
            {
                record.EndTime = endTime;
            }

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
        await InitializeDriverAsync();

        await _page.GotoAsync($"{Constants.DigiSheetBaseUrl}randstad/staffLogin");

        await _page.FillAsync("input[name=HC]", Constants.DigiSheetStaffingAgencyCd);
        await _page.FillAsync("input[name=UI]", username);
        await _page.FillAsync("input[name=Pw]", password);
        await _page.ClickAsync("input[name=loginButton]");

        await _page.WaitForSelectorAsync("frame[name=menu]", _pageWaitForSelectorOptions);
    }

    public async Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        var dateNameFormat = date.ToString("yyyy-MM-dd");
        var inputElements = _currentFrameLocator.Locator("//input[starts-with(@name, 'DayTable_WorkDate')]");
        var count = await inputElements.CountAsync();
        var dateFound = false;

        for (int i = 0; i < count; i++)
        {
            var inputElement = inputElements.Nth(i);
            if (await inputElement.GetAttributeAsync("value") == dateNameFormat)
            {
                var trElement = await inputElement.Locator("xpath=../..").ElementHandleAsync();
                var anchorTd = await trElement.QuerySelectorAsync("td:nth-child(3)");
                var anchor = await anchorTd.QuerySelectorAsync("a");
                await anchor.ClickAsync();
                dateFound = true;
                await KeyInDetail(startTime, endTime, remarks);
                await SubmitChangesAsync();
                break;
            }
        }

        if (!dateFound)
        {
            throw new Exception($"Date {dateNameFormat} not found in the attendance report");
        }
    }

    private async Task SubmitChangesAsync()
    {
        await _currentFrameLocator.Locator("//input[@type='checkbox']").WaitForAsync();
        var checkBoxes = _currentFrameLocator.Locator("//input[@type='checkbox']");
        var count = await checkBoxes.CountAsync();
        for (int i = 0; i < count; i++)
        {
            await checkBoxes.Nth(i).ClickAsync();
        }

        var updateButton = _currentFrameLocator.Locator("//input[@value='更　新']");
        await updateButton.First.ClickAsync();
    }

    private async Task KeyInDetail(TimeOnly startTime, TimeOnly endTime, string remarks)
    {
        // Wait for the RegistButton inside the iframe to appear
        await _currentFrameLocator.Locator("input[name=RegistButton]").WaitForAsync();

        // Interact with elements inside the iframe using _currentFrameLocator and Locator
        await _currentFrameLocator.Locator("select[name=HourStart]").SelectOptionAsync(startTime.Hour.ToString());
        await _currentFrameLocator.Locator("input[name=MinuteStart]").FillAsync(startTime.Minute.ToString());

        await _currentFrameLocator.Locator("select[name=HourEnd]").SelectOptionAsync(endTime.Hour.ToString());
        await _currentFrameLocator.Locator("input[name=MinuteEnd]").FillAsync(endTime.Minute.ToString());

        // Handle the restAdjust button, if it exists
        try
        {
            await _currentFrameLocator.Locator("input[name=restAdjust]").ClickAsync();
        }
        catch (Exception)
        {
            // Ignore if restAdjust is not found
        }
        //todo after restAdjust button is clicked, there is a loading spinner which prevents the next action from being executed
        await Task.Delay(1000);

        // Fill in the remarks and select rest time
        await _currentFrameLocator.Locator("input[name=CommentInput]").FillAsync(remarks);
        await _currentFrameLocator.Locator("select[name=HourRest]").SelectOptionAsync("1");

        // Click the RegistButton to submit the details
        await _currentFrameLocator.Locator("input[name=RegistButton]").ClickAsync();

        //await _page.WaitForSelectorAsync("input[name=RegistButton]");
        //await _page.SelectOptionAsync("select[name=HourStart]", startTime.Hour.ToString());
        //await _page.FillAsync("input[name=MinuteStart]", startTime.Minute.ToString());

        //await _page.SelectOptionAsync("select[name=HourEnd]", endTime.Hour.ToString());
        //await _page.FillAsync("input[name=MinuteEnd]", endTime.Minute.ToString());

        //try
        //{
        //    await _page.ClickAsync("input[name=restAdjust]");
        //}
        //catch (Exception)
        //{
        //    // Ignore if restAdjust is not found
        //}

        //await _page.FillAsync("input[name=CommentInput]", remarks);
        //await _page.SelectOptionAsync("select[name=HourRest]", "1");
        //await _page.ClickAsync("input[name=RegistButton]");
    }

    public void Dispose()
    {
        if (_browser != null)
        {
            _browser.CloseAsync().Wait();
        }
    }
}
