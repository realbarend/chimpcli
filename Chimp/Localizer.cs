using System.Globalization;
using Chimp.Models.Api;

namespace Chimp;

public enum SupportedUiLanguage { Nl, En }

public class Localizer
{
    private readonly string _chimpLanguage;
    private readonly SupportedUiLanguage _cliLanguage;
    
    public Localizer(string chimpLanguage)
    {
        _chimpLanguage = chimpLanguage;
        if (!Enum.TryParse<SupportedUiLanguage>(chimpLanguage, true, out var uiLanguage)) uiLanguage = SupportedUiLanguage.En;
        _cliLanguage = uiLanguage;
    }

    public string GetToday() =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => "VANDAAG",
            _ => "TODAY"
        };

    public string GetWeekDay(DateTime date) => string.Create(CultureInfo.GetCultureInfo(_chimpLanguage), $"{date:dddd}");
    public string GetLongDate(DateTime date) => string.Create(CultureInfo.GetCultureInfo(_chimpLanguage), $"{date:D}");

    public string GetTimeSheetRow(TimeSheetViewRow row) =>
        $"{$"[{row.Line,2}] {row.TaskName} ({row.ProjectName}) p{row.ProjectLine}",-60} {row.Start?.ToLocalTime():HH:mm}-{row.End?.ToLocalTime():HH:mm} ({Util.HoursNotation(row.Hours),4}) {row.Notes}";
    
    public string GetDaySummary(double weekTotal, double billableTotal, string displayDay, double dayTotalHours) =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => $"{$"DEZE WEEK {weekTotal} uren waarvan {billableTotal} facturabel",-60} {displayDay} {dayTotalHours} uren",
            _ => $"{$"CURRENT WEEK {weekTotal} hours of which {billableTotal} billable",-60} {displayDay} {dayTotalHours} hours"
        };

    public string GetTimeTravelerAlert(int weeks) =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => $"** TIJDREIZIGER ALARM  ---  je bevindt je op dit moment {Math.Abs(weeks)} {(Math.Abs(weeks) < 2 ? "week" : "weken")} {(weeks < 0 ? "achter op" : "vooruit op").ToUpperInvariant()} normale tijd **",
            _ => $"** TIME TRAVELER ALERT  ---  you are currently {Math.Abs(weeks)} {(Math.Abs(weeks) < 2 ? "week" : "weeks")} {(weeks < 0 ? "behind" : "ahead of").ToUpperInvariant()} normal time **"
        };
    
    public string GetDeleteConfirmation(int line) =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => $"Urenregel #{line} wordt verwijderd: weet je het zeker? J/N",
            _ => $"About to delete row #{line}: are you sure? Y/N"
        };

    public ConsoleKey GetYesKey() =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => ConsoleKey.J,
            _ => ConsoleKey.Y
        };
    
    public string GetDeleteAborted() =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => "Niet verwijderd.",
            _ => "Aborting."
        };
}
