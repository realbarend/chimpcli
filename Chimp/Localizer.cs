using System.Globalization;
using System.Text.RegularExpressions;
using Chimp.Models;

namespace Chimp;

public enum SupportedUiLanguage { Nl, En }

public class Localizer
{
    private readonly SupportedUiLanguage _cliLanguage;
    public CultureInfo ChimpCulture { get; }
    
    public Localizer(string chimpLanguage)
    {
        if (!Enum.TryParse<SupportedUiLanguage>(chimpLanguage, true, out var uiLanguage)) uiLanguage = SupportedUiLanguage.En;
        _cliLanguage = uiLanguage;

        try
        {
            ChimpCulture = CultureInfo.GetCultureInfo(chimpLanguage);
        }
        catch (CultureNotFoundException)
        {
            ChimpCulture = CultureInfo.InvariantCulture;
        }
    }

    public string GetHelpMessage() => _cliLanguage switch
    {
        SupportedUiLanguage.Nl =>
            """
            TimeChimp CLI for tracking hours
            Get more information at https://github.com/realbarend/chimpcli

            Parameters:
            login, l                                   # login met je timechimp gebruikersnaam en wachtwoord
            projects, p                                # toon alle beschikbare projecten en labels
            add, a <projectSpec> <timeSpec> <notes>    # voeg een regel toe
            ls (this is the default)                   # bekijk de urenregels van de actieve week
            week, w <week-offset>                      # wijzig de actieve week naar een andere week
            update, u <line#> <new-notes>              # wijzig de notitie van een bestaande urenregel
            update, u <line#> <new-timespec>           # wijzig het tijdsinterval van een bestaande urenregel
            update, u <line#> <new-projectspec>        # wijzig het projectnummer en/of labels van een bestaande urenregel
            delete, del, d <line#>                     # verwijder de urenregel (vraagt om confirmatie)

            <projectSpec>: pXX                         # specificeert een projectnummer (zie het 'projects' commando)
            <projectSpec>: pXX-A or pXX-A,B            # het projectnummer mag labelnummers bevatten: 'A' or 'A,B' etc
            <timeSpec>: from-to                        # specificeert een tijdinterval met eindtijd, bijv. 8.30-10.00
            <timeSpec>: from+minutes                   # specificeert een tijdinterval met aantal minuten, bijv. 8.30+30 -> 8.30-9.00
            <timeSpec>: dd:from-to or dd:from+minutes  # het tijdinterval mag een dag-voorvoegsel bevatten, bijv. 'vr:' voor vrijdag

            Voorbeelden:
            chimp login                                # log in voordat je iets anders probeert
            chimp projects                             # bekijk projecten en labels: de nummers heb je nodig bij de andere commmando's
            chimp add p12 9.30+15 mijn notitie         # invoer: gewerkt op project '12' aan 'mijn notitie' van 09:30 tot 9:45
            chimp a p12 ma:9:30+15 note bla bla        # invoer: voeg een regel toe aan 'maandag' in plaats van 'vandaag'
            chimp                                      # bekijk alle ingevoerde uren van de actieve week
            chimp week -1                              # wijzig actieve week naar 'vorige week' (dit wordt onthouden)
            chimp update 5 updating my note            # wijzig de notitie van urenregel nummer 5 naar 'updating my note'
            chimp u 5 930-10                           # wijzig het tijdsinterval van urenregel 5 naar 9.30-10.00
            chimp u 5 fr:930-10                        # verplaats urenregel 5 naar 'vrijdag' en wijzig de tijd naar 9.30-10.00
            chimp update 5 p2-3,5                      # wijzig het projectnummer van urenregel 5 naar 'p2' en zet labelnummers '2' en '3'
            chimp del 5                                # verwijder urenregel nummer 5 (vraagt om confirmatie)
            """,
        _ =>
            """
            TimeChimp CLI for tracking hours
            Get more information at https://github.com/realbarend/chimpcli

            Parameters:
            login, l                                   # login with your timechimp username and password
            projects, p                                # display all available projects and tags
            add, a <projectSpec> <timeSpec> <notes>    # add a new record
            ls (this is the default)                   # list currently tracked hours for the active week
            week, w <week-offset>                      # change the active week to a different week
            update, u <line#> <new-notes>              # update the notes for an existing record
            update, u <line#> <new-timespec>           # update the time interval for an existing record
            update, u <line#> <new-projectspec>        # update the project number and/or tags for an existing record
            delete, del, d <line#>                     # make the record be gone (will ask for confirmation)

            <projectSpec>: pXX                         # specifies the project number (see 'projects' command)
            <projectSpec>: pXX-A or pXX-A,B            # the project number may include tag numbers: 'A' or 'A,B' etc
            <timeSpec>: from-to                        # specifies a time interval with end time, eg. 8.30-10.00
            <timeSpec>: from+minutes                   # specifies a time-interval with minute count, eg. 8.30+30 -> 8.30-9.00
            <timeSpec>: dd:from-to or dd:from+minutes  # the time-interval may include a day interval, eg 'fr:' for Friday

            Examples:
            chimp login                                # you should login before trying anything else
            chimp projects                             # list projects and tags: you need the numbers when using other commands
            chimp add p12 9.30+15 my notes             # input: started working on project '12' on 'my notes' starting 09:30, ended at 9:45
            chimp a p12 mo:9.30+15 note bla bla        # input: track hours for 'monday' instead of 'today'
            chimp                                      # list all tracked hours for the active week
            chimp week -1                              # change the active week to 'last week' (this will be remembered)
            chimp update 5 updating my note            # change the note for record with number 5 to 'updating my note'
            chimp u 5 930-10                           # change the time interval for record 5 to 9.30-10.00
            chimp u 5 fr:930-10                        # move record 5 to 'Friday' and change the time to 9.30-10.00
            chimp update 5 p2-3,5                      # change the project number for record 5 to '2' and also set tag numbers '3' and '5'
            chimp del 5                                # make the fifth record be gone (will ask for confirmation)
            """
    };

    public string TranslateLiteral(string literal, Dictionary<string, object>? args = null)
    {
        var translated = literal switch
        {
            "About to delete row #{Line}: are you sure? Y/N" when _cliLanguage == SupportedUiLanguage.Nl => "Urenregel #{Line} wordt verwijderd: weet je het zeker? J/N.",
            "Available projects" when _cliLanguage == SupportedUiLanguage.Nl => "Beschikbare projecten",
            "Available tags" when _cliLanguage == SupportedUiLanguage.Nl => "Beschikbare labels",
            "cannot parse timeSpec '{TimeSpec}'" when _cliLanguage == SupportedUiLanguage.Nl => "kan timeSpec '{TimeSpec}' niet parsen",
            "could not parse the weekdayprefix: use 'mo:', 'tu:', 'we:', 'th:' or 'fr:'" when _cliLanguage == SupportedUiLanguage.Nl => "kan de weekdag niet parsen: gebruik 'ma:', 'di:', 'wo:', 'do:' of 'vr:'",
            "expected '{ParamName}' parameter missing" when _cliLanguage == SupportedUiLanguage.Nl => "verwachte parameter '{ParamName}' ontbreekt",
            "hour '{Hour}' is invalid" when _cliLanguage == SupportedUiLanguage.Nl => "uur '{Hour}' is ongeldig",
            "invalid projectSpec '{ProjectSpec}', use pNN or pNN-A,B" when _cliLanguage == SupportedUiLanguage.Nl => "ongeldige projectSpec '{ProjectSpec}', gebruik pNN of pNN-A,B",
            "invalid weekOffset: time travel is allowed for maximum 52 weeks" when _cliLanguage == SupportedUiLanguage.Nl => "ongeldige weekOffset: tijdreizen kan maximaal tot 52 weken",
            "login attempt failed, maybe wrong password. note: timechimp can temporarily block your account after multiple failures" when _cliLanguage == SupportedUiLanguage.Nl => "inlogpoging mislukt, misschien verkeerd wachtwoord. letop: timechimp kan bij meerdere inlogfouten tijdelijk je account blokkeren",
            "Login successful, got authtoken valid until {ExpireDate}" when _cliLanguage == SupportedUiLanguage.Nl => "Successvol ingelogd, kreeg authtoken geldig tot {ExpireDate}",
            "minute '{Minute}' is invalid" when _cliLanguage == SupportedUiLanguage.Nl => "minuut '{Minute}' is ongeldig",
            "Not removed." when _cliLanguage == SupportedUiLanguage.Nl => "Niet verwijderd.",
            "parameter '{ParamName}' must be a number" when _cliLanguage == SupportedUiLanguage.Nl => "parameter '{ParamName}' moet een getal zijn",
            "password empty: cannot login" when _cliLanguage == SupportedUiLanguage.Nl => "wachtwoord leeg: kan niet inloggen",
            "previously fetched projects list does not contain line #{Line}" when _cliLanguage == SupportedUiLanguage.Nl => "de eerder opgehaalde projectlijst bevat geen regel met nummer #{Line}",
            "previously fetched timesheet does not contain line #{Line}" when _cliLanguage == SupportedUiLanguage.Nl => "de eerder opgehaalde urenlijst bevat geen regel met nummer #{Line}",
            "previously fetched tags list does not contain line #{Line}" when _cliLanguage == SupportedUiLanguage.Nl => "de eerder opgehaalde taglijst bevat geen regel met nummer #{Line}",
            "timeSpec '{TimeSpec}' issue: end time should be after start time" when _cliLanguage == SupportedUiLanguage.Nl => "probleem met timeSpec '{TimeSpec}': eindtijd moet groter zijn dan starttijd",
            "timeSpec '{TimeSpec}' issue: start and end must be on the same day" when _cliLanguage == SupportedUiLanguage.Nl => "probleem met timeSpec '{TimeSpec}': start- en eindtijd moeten op dezelfde dag vallen",
            "timeSpec '{TimeSpec}' issue: to help preventing input mistakes, a time interval is not allowed not exceed 10 hours" when _cliLanguage == SupportedUiLanguage.Nl => "probleem met timeSpec '{TimeSpec}': om invoerfouten te helpen voorkomen, mag een tijdsinterval niet groter zijn dan 10 uur",
            "TODAY" when _cliLanguage == SupportedUiLanguage.Nl => "VANDAAG",
            "try 'chimp help' to get help" when _cliLanguage == SupportedUiLanguage.Nl => "probeer 'chimp help' voor meer uitleg",
            "username empty: cannot login" when _cliLanguage == SupportedUiLanguage.Nl => "gebruikersnaam leeg: kan niet inloggen",
            "when time traveling, the timeSpec must always include the weekday, eg 'fr:{TimeSpec}'" when _cliLanguage == SupportedUiLanguage.Nl => "bij tijdreizen, moet de timeSpec altijd de weekdag bevatten, bijv 'vr:{TimeSpec}'",
            "you must first login" when _cliLanguage == SupportedUiLanguage.Nl => "je moet eerst inloggen",
            "you must first fetch the project list" when _cliLanguage == SupportedUiLanguage.Nl => "je moet eerst de projectlijst ophalen",
            "you must first fetch the timesheet" when _cliLanguage == SupportedUiLanguage.Nl => "je moet eerst de urenlijst ophalen",
            _ when _cliLanguage == SupportedUiLanguage.En => literal,
#if DEBUG
            _ => throw new ApplicationException($"Missing translation for '{literal}'"),
#else
            _ => literal,
#endif
        };
        return args != null ? Interpolate(translated) : translated;
        
        string Interpolate(string s) => Regex.Replace(s, @"{([^}]+)}", match => args.TryGetValue(match.Groups[1].Value, out var value) ? value.ToString() ?? string.Empty : match.Value);
    }

    public string GetWeekDay(DateTime date) => string.Create(ChimpCulture, $"{date:dddd}");
    public string GetLongDate(DateTime date) => string.Create(ChimpCulture, $"{date:D}");

    public string GetTimeSheetRow(TimeSheetRowViewModel row)
    {
        var firstColumn = $"[{row.Line,2}] {row.ProjectName} {row.ProjectSpec}";
        var tags = !string.IsNullOrEmpty(row.Tags) ? $" [tags: {row.Tags}]" : "";
        return $"{firstColumn,-60} {row.Start?.ToLocalTime():HH:mm}-{row.End?.ToLocalTime():HH:mm} ({Util.HoursNotation(row.Hours),4}) {row.Notes}{tags}";
    }

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

    public ConsoleKey GetYesKey() =>
        _cliLanguage switch
        {
            SupportedUiLanguage.Nl => ConsoleKey.J,
            _ => ConsoleKey.Y
        };
}
