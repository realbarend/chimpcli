using Chimp.Services;

namespace Chimp.Shell;

public interface IShellCommand
{
    Task Handle(TimeSheetService service);
}
