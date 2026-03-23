using Chimp.Common;
using Chimp.Services;
using static Chimp.Shell.Localization;

namespace Chimp.Shell;

public class LoginCommand : IShellCommand
{
    public bool PersistCredentials { get; init; }
    public string? Password { get; init; }
    public string? User { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        await HandleLogin(service);
        await new ListProjectsCommand().Handle(service);
        await new ListTimeSheetCommand().Handle(service);
    }

    private async Task HandleLogin(TimeSheetService service)
    {
        // If credentials were passed to the command, then we prefer to use those for login.
        if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
        {
            await service.Login(User, Password, PersistCredentials);
            WriteLocalized("** successfully logged in as {User}", new { User = User });
            return;
        }

        // Next option: if we have previously persisted credentials, use those.
        if (await service.TryLoginUsingPersistedCredentials())
        {
            WriteLocalized("** successfully logged in using persisted credentials");
            return;
        }

        // Final option: do (partial) interactive login.
        var user = User;
        if (string.IsNullOrEmpty(user))
        {
            Console.Write("Enter your timechimp username: ");
            user = Console.ReadLine();
            if (string.IsNullOrEmpty(user)) throw new Error("username empty: cannot login");
        }

        var password = Password;
        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Enter your timechimp password: ");
            password = Console.ReadLine();
            if (string.IsNullOrEmpty(password)) throw new Error("password empty: cannot login");
        }

        await service.Login(user, password, PersistCredentials);
        WriteLocalized("** successfully logged in as {User}", new { User = user });
    }
}
