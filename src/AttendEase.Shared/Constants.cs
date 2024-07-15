using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendEase.Shared;
public class Constants
{
    public const int MaxRetries = 3;
    public const int RetryDelay = 1000;
    public const int PageTimeoutSeconds = 10;
    public const string FmNetBaseUrl = "https://fm-net.furukawa.co.jp/";
    public const string DigiSheetBaseUrl = "https://www6.digisheet.com/";
    public const string DigiSheetStaffingAgencyCd = "5018";
}
