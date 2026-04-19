using System.Reflection;
using System.Text;
using Chimp.Api;
using Chimp.Common;
using Chimp.Services;
using Chimp.Shell;
using static Chimp.Shell.Localization;

namespace Chimp;

internal static class Program
{
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    public static async Task Main()
    {
        IEnvironment environment = new SystemEnvironment();

        Console.OutputEncoding = Encoding.UTF8;
        var logger = new DebugLogger(environment.DebugEnabled ? Console.Out : new StreamWriter(Stream.Null));

        var stateFilePath = Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chimpcli");
        var stateBag = InitializeStateBag(stateFilePath, logger);
        var authentication = new CognitoAuthentication(stateBag, new HttpClient(), logger);
        var timeSheetService = new TimeSheetService(stateBag, new Client(stateBag, authentication, new HttpClient { BaseAddress = new Uri("https://web.timechimp.com/api/") }, logger), logger);
        LanguageHelper.SetUiLanguage(environment.GetEnvironmentVariable(environment.ChimpCliLanguage) ?? timeSheetService.GetUserLanguage());

        try
        {
            var command = new CommandParser(environment, logger).ParseCommandLine();
            await command.Handle(timeSheetService);
        }
        catch (Error error)
        {
            Console.WriteLine($"Error: {Localize(error.Message, error.Args)}");
            if (environment.DebugEnabled)
            {
                if (error.InnerException != null) Console.WriteLine(error.InnerException.Message);
                Console.WriteLine(error.StackTrace);
            }
            else
            {
                WriteLocalized("Setting environment variable {EnableDebug}=1 may show more details.", environment.ChimpCliDebug);
            }

            Console.WriteLine();
            WriteLocalized("Try 'chimp help' to get help.");
        }
        finally
        {
            await new UpdateChecker(stateBag).CheckAndNotify();

            stateBag.Save();
        }
    }

    private static PersistablePropertyBag InitializeStateBag(string stateFilePath, DebugLogger logger)
    {
        PersistablePropertyBag? appState = null;
        try
        {
            appState = PersistablePropertyBag.ReadFromDisk(stateFilePath);
        }
        catch (FileNotFoundException)
        {
            logger.Log("** could not find state-file -- this should get solved when logging in");
        }
        catch (Exception e)
        {
            // don't throw here because that would block the login flow
            WriteLocalized("** failed to read state-file: {Message} -- try logging in again or manually remove the file", e.Message);
        }

        return appState ?? PersistablePropertyBag.CreateNew(stateFilePath);
    }
}
