using Chimp.Services;

namespace Chimp.Shell;
using static Localization;

public class HelpCommand : IShellCommand
{
    public Task Handle(TimeSheetService service)
    {
        WriteHelpText();
        return Task.CompletedTask;
    }
}
