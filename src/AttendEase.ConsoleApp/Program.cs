﻿using AttendEase.Shared.Services;

Console.WriteLine("Enter your username:");
var username = Console.ReadLine();
Console.WriteLine("Enter your password:");
var pass = string.Empty;
ConsoleKey key;
do
{
    var keyInfo = Console.ReadKey(intercept: true);
    key = keyInfo.Key;

    if (key == ConsoleKey.Backspace && pass.Length > 0)
    {
        Console.Write("\b \b");
        pass = pass[0..^1];
    }
    else if (!char.IsControl(keyInfo.KeyChar))
    {
        Console.Write("*");
        pass += keyInfo.KeyChar;
    }
} while (key != ConsoleKey.Enter);
try
{
    IDigiSheetService digiSheetService = new DigiSheetSeleniumService();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}