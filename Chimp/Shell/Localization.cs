using System.Globalization;

namespace Chimp.Shell;

public static class Localization
{
    private enum UiLanguage { Nl, En }
    private static UiLanguage GetLanguage() => Enum.TryParse<UiLanguage>(CultureInfo.CurrentUICulture.Name, true, out var uiLanguage) ? uiLanguage :  UiLanguage.En;

    public static void WriteLocalized(string literal, object? args = null) => Console.WriteLine(Localize(literal, args));

    public static string Localize(string literal, object? args = null)
    {
        var language = GetLanguage();
        var translated = literal switch
        {
            "** TIME TRAVELER ALERT  ---  you are currently {WeekCount} {Weeks} {AheadOrBehind} normal time **"
                when language == UiLanguage.Nl => "** TIJDREIZIGER ALARM  ---  je bevindt je op dit moment {WeekCount} {Weeks} {AheadOrBehind} normale tijd **",
            "week" when language == UiLanguage.Nl => "week",
            "weeks" when language == UiLanguage.Nl => "weken",
            "behind" when language == UiLanguage.Nl => "achter op",
            "ahead of" when language == UiLanguage.Nl => "vooruit op",

            "CURRENT WEEK {TotalHours} hours of which {BillableTotal} billable"
                when language == UiLanguage.Nl => "DEZE WEEK {TotalHours} uren waarvan {BillableTotal} facturabel",
            "hours" when language == UiLanguage.Nl => "uren",

            "** failed to read state-file: {Message} -- try logging in again or manually remove the file"
                when language == UiLanguage.Nl => "** kan de state-file niet lezen: {Message} -- probeer opnieuw in te loggen of verwijder het bestand handmatig",

            "** successfully logged in as {User}"
                when language == UiLanguage.Nl => "** succesvol ingelogd als {User}",
            "** successfully logged in using persisted credentials"
                when language == UiLanguage.Nl => "** succesvol ingelogd met eerder opgeslagen wachtwoord",

            "** refresh: got new accesstoken valid until {Date}"
                when language == UiLanguage.Nl => "** refresh: kreeg nieuw accesstoken geldig tot {Date}",
            "** refresh: refreshing the accesstoken failed: {Message}"
                when language == UiLanguage.Nl => "** refresh: het refreshen van het accesstoken is mislukt: {Message}",

            "About to delete row #{Line}: are you sure? Y/N" when language == UiLanguage.Nl => "Urenregel #{Line} wordt verwijderd: weet je het zeker? J/N.",
            "Available projects" when language == UiLanguage.Nl => "Beschikbare projecten",
            "Available tags" when language == UiLanguage.Nl => "Beschikbare labels",
            "cannot parse timeEntry '{TimeEntry}'" when language == UiLanguage.Nl => "kan timeEntry '{TimeEntry}' niet parsen",
            "could not parse the weekdayprefix: use 'mo:', 'tu:', 'we:', 'th:' or 'fr:'" when language == UiLanguage.Nl => "kan de weekdag niet parsen: gebruik 'ma:', 'di:', 'wo:', 'do:' of 'vr:'",
            "expected '{ParamName}' parameter missing" when language == UiLanguage.Nl => "verwachte parameter '{ParamName}' ontbreekt",
            "hour '{Hour}' is invalid" when language == UiLanguage.Nl => "uur '{Hour}' is ongeldig",
            "invalid project '{ProjectAlias}', use pNN or pNN-A,B" when language == UiLanguage.Nl => "ongeldig project '{ProjectAlias}', gebruik pNN of pNN-A,B",
            "invalid weekOffset: time travel is allowed for maximum 52 weeks" when language == UiLanguage.Nl => "ongeldige weekOffset: tijdreizen kan maximaal tot 52 weken",
            "login attempt failed: {Message}. Note: timechimp may temporarily block your account after multiple failures." when language == UiLanguage.Nl => "inlogpoging mislukt: {Message}. Let op: timechimp kan bij meerdere inlogfouten tijdelijk je account blokkeren.",
            "could not finish login: unsupported challenge: {Challenge}" when language == UiLanguage.Nl => "kan het inloggen niet voltooien: niet-ondersteunde challenge: {Challenge}",
            "minute '{Minute}' is invalid" when language == UiLanguage.Nl => "minuut '{Minute}' is ongeldig",
            "Not removed." when language == UiLanguage.Nl => "Niet verwijderd.",
            "parameter '{ParamName}' must be a number" when language == UiLanguage.Nl => "parameter '{ParamName}' moet een getal zijn",
            "password empty: cannot login" when language == UiLanguage.Nl => "wachtwoord leeg: kan niet inloggen",
            "2fa code empty: cannot finish login" when language == UiLanguage.Nl => "2fa code leeg: kan het inloggen niet voltooien",
            "previously fetched projects list does not contain line #{Line}" when language == UiLanguage.Nl => "de eerder opgehaalde projectlijst bevat geen regel met nummer #{Line}",
            "previously fetched timesheet does not contain line #{Line}" when language == UiLanguage.Nl => "de eerder opgehaalde urenlijst bevat geen regel met nummer #{Line}",
            "previously fetched tags list does not contain tag {Tag}" when language == UiLanguage.Nl => "de eerder opgehaalde taglijst bevat geen tag {Tag}",
            "timeEntry '{TimeEntry}' issue: end time should be after start time" when language == UiLanguage.Nl => "probleem met timeEntry '{TimeEntry}': eindtijd moet groter zijn dan starttijd",
            "timeEntry '{TimeEntry}' issue: start and end must be on the same day" when language == UiLanguage.Nl => "probleem met timeEntry '{TimeEntry}': start- en eindtijd moeten op dezelfde dag vallen",
            "timeEntry '{TimeEntry}' issue: to help preventing input mistakes, a time interval is not allowed not exceed 10 hours" when language == UiLanguage.Nl => "probleem met timeEntry '{TimeEntry}': om invoerfouten te helpen voorkomen, mag een tijdsinterval niet groter zijn dan 10 uur",
            "TODAY" when language == UiLanguage.Nl => "VANDAAG",
            "Try 'chimp help' to get help." when language == UiLanguage.Nl => "Probeer 'chimp help' voor uitleg.",
            "username empty: cannot login" when language == UiLanguage.Nl => "gebruikersnaam leeg: kan niet inloggen",
            "when time traveling, the timeEntry must always include the weekday, eg 'fr:{TimeEntry}'" when language == UiLanguage.Nl => "bij tijdreizen, moet de timeEntry altijd de weekdag bevatten, bijv 'vr:{TimeEntry}'",
            "you must first login" when language == UiLanguage.Nl => "je moet eerst inloggen",
            "you must first fetch the project list" when language == UiLanguage.Nl => "je moet eerst de projectlijst ophalen",
            "you must first fetch the timesheet" when language == UiLanguage.Nl => "je moet eerst de urenlijst ophalen",

            "cannot copy row #{Line}, because the project or tag is not available" when language == UiLanguage.Nl => "kan rij #{Line} niet kopieren, omdat het project of tag niet beschikbaar is",

            "api returned httpcode {Code} ({CodeString}): if this persists, try to login" when language == UiLanguage.Nl => "api geeft httpcode {Code} ({CodeString}): misschien moet je opnieuw inloggen",

            "Setting environment variable {EnableDebug}=1 may show more details." when language == UiLanguage.Nl => "Gebruik omgevingsvariabele {EnableDebug}=1 om mogelijk meer details te krijgen.",

            _ when language == UiLanguage.En => literal,
#if DEBUG
            _ => throw new ApplicationException($"Missing translation: {literal}"),
#else
            _ => literal,
#endif
        };

        var replacements = (args?.GetType().GetProperties() ?? []).ToDictionary(p => "{" + p.Name + "}", p => p.GetValue(args)?.ToString());
        foreach (var replacement in replacements) translated = translated.Replace(replacement.Key, replacement.Value);

        return translated;
    }

    public static void WriteHelpText()
    {
        var helpText = GetLanguage() switch
        {
            UiLanguage.Nl =>
                            """
            TimeChimp CLI om uren bij te houden
            Meer informatie: https://github.com/realbarend/chimpcli

            Parameters:
            login, l                                   # login met je timechimp gebruikersnaam en wachtwoord
            projects, p                                # toon alle beschikbare projecten en labels
            add, a <projectAlias> <timeEntry> <notes>  # voeg een regel toe
            ls (this is the default)                   # bekijk de urenregels van de actieve week
            week, w <week-offset>                      # wijzig de actieve week naar een andere week
            update, u <line#> <new-notes>              # wijzig de notitie van een bestaande urenregel
            update, u <line#> <new-timeEntry>          # wijzig het tijdsinterval van een bestaande urenregel
            update, u <line#> <new-projectAlias>       # wijzig het projectnummer en/of labels van een bestaande urenregel
            delete, del, d <line#>                     # verwijder de urenregel (vraagt om confirmatie)

            <projectAlias>: pXX                         # specificeert een projectnummer (zie het 'projects' commando)
            <projectAlias>: pXX-A or pXX-A,B            # het projectnummer mag labelnummers bevatten: 'A' or 'A,B' etc
            <timeEntry>: from-to                        # specificeert een tijdinterval met eindtijd, bijv. 8.30-10.00
            <timeEntry>: from+minutes                   # specificeert een tijdinterval met aantal minuten, bijv. 8.30+30 -> 8.30-9.00
            <timeEntry>: dd:from-to or dd:from+minutes  # het tijdinterval mag een dag-voorvoegsel bevatten, bijv. 'vr:' voor vrijdag

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
            add, a <projectAlias> <timeEntry> <notes>  # add a new record
            ls (this is the default)                   # list currently tracked hours for the active week
            week, w <week-offset>                      # change the active week to a different week
            update, u <line#> <new-notes>              # update the notes for an existing record
            update, u <line#> <new-timeEntry>          # update the time interval for an existing record
            update, u <line#> <new-projectAlias>       # update the project number and/or tags for an existing record
            delete, del, d <line#>                     # make the record be gone (will ask for confirmation)

            <projectAlias>: pXX                         # specifies the project number (see 'projects' command)
            <projectAlias>: pXX-A or pXX-A,B            # the project number may include tag numbers: 'A' or 'A,B' etc
            <timeEntry>: from-to                        # specifies a time interval with end time, eg. 8.30-10.00
            <timeEntry>: from+minutes                   # specifies a time-interval with minute count, eg. 8.30+30 -> 8.30-9.00
            <timeEntry>: dd:from-to or dd:from+minutes  # the time-interval may include a day interval, eg 'fr:' for Friday

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
            """,
        };

        Console.WriteLine(helpText);
    }

    public static bool ReadLocalizedYesKey()
    {
        var yesKey = GetLanguage() switch
        {
            UiLanguage.Nl => ConsoleKey.J,
            _ => ConsoleKey.Y,
        };

        return Console.ReadKey(true).Key == yesKey;
    }
}
