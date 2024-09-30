using AttendEase.Shared.Enums;
using AttendEase.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace AttendEase.Shared.Services;

public class FmNetPlaywrightService : IFmNetService
{
    private IBrowser _browser;
    private IPage _page;

    private List<string> GetAttendanceRowsFromAttendanceTableHtml(string attendanceTableHtml)
    {
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

        var record = new AttendanceRecord();

        var tds = doc.DocumentNode.SelectNodes("//td");

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

            // Extract start and end times
            var actualTimeSpans = tds[4].SelectNodes(".//span");
            if (actualTimeSpans?.Count >= 6)
            {
                int startHour = int.Parse(actualTimeSpans[1].InnerText);
                int startMinute = int.Parse(actualTimeSpans[2].InnerText);
                int endHour = int.Parse(actualTimeSpans[4].InnerText);
                int endMinute = int.Parse(actualTimeSpans[5].InnerText);
                record.ActualStartTime = new TimeOnly(startHour, startMinute);
                record.ActualEndTime = new TimeOnly(endHour, endMinute);
            }

            var timeSpans = tds[5].SelectNodes(".//span");
            if (timeSpans?.Count >= 6)
            {
                int startHour = int.Parse(timeSpans[1].InnerText);
                int startMinute = int.Parse(timeSpans[2].InnerText);
                int endHour = int.Parse(timeSpans[4].InnerText);
                int endMinute = int.Parse(timeSpans[5].InnerText);
                record.StartTime = new TimeOnly(startHour, startMinute);
                record.EndTime = new TimeOnly(endHour, endMinute);
            }

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

    private async Task InitializeDriverAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecords(DateOnly date)
    {
        await _page.ClickAsync("//a[.//descendant::*[contains(text(), '就労管理')]]");

        await _page.ClickAsync(".today");

        await _page.WaitForSelectorAsync("//div[contains(text(), '勤務実績入力')]");
        await _page.ClickAsync("//div[contains(text(), '勤務実績入力')]");

        await _page.WaitForSelectorAsync("table[name='APPROVALGRD']");
        var attendanceTableHtml = await _page.GetAttributeAsync("table[name='APPROVALGRD']", "innerHTML");

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
        }
        return records;
    }

    public async Task Login(string username, string password)
    {
        await InitializeDriverAsync();

        await _page.GotoAsync($"{Constants.FmNetBaseUrl}self-workflow/cws/cws");

        await _page.FillAsync("input[name='uid']", username);
        await _page.FillAsync("input[name='pwd']", password);
        await _page.ClickAsync("input[name='Login']");

        await _page.WaitForSelectorAsync("#logout");
    }

    public async Task SubmitAttendance(DateOnly date, TimeOnly startTime, TimeOnly endTime, string remarks = "")
    {
        await _page.WaitForSelectorAsync("table[name='APPROVALGRD']");

        var inputNameFormat = date.ToString("yyyy_M_d");
        var startHourInputName = $"K{inputNameFormat}0STH";
        var startMinuteInputName = $"K{inputNameFormat}0STM";
        var endHourInputName = $"K{inputNameFormat}0ETH";
        var endMinuteInputName = $"K{inputNameFormat}0ETM";
        var submitButtonName = $"BTNDCDS{inputNameFormat}0";

        await _page.SelectOptionAsync($"#{startHourInputName}", startTime.Hour.ToString());
        await _page.SelectOptionAsync($"#{startMinuteInputName}", startTime.Minute.ToString());
        await _page.SelectOptionAsync($"#{endHourInputName}", endTime.Hour.ToString());
        await _page.SelectOptionAsync($"#{endMinuteInputName}", endTime.Minute.ToString());

        await _page.ClickAsync($"#{submitButtonName}");
        await _page.WaitForSelectorAsync("#dSubmission1");
        await _page.ClickAsync("#dSubmission1");
    }
    public void Dispose()
    {
        if (_browser != null)
        {
            _browser.CloseAsync();
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
    }
}
