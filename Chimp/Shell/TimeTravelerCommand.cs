using Chimp.Services;

namespace Chimp.Shell;

public class TimeTravelerCommand : IShellCommand
{
    public int WeekOffset { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        service.SetWeekOffset(WeekOffset);

        await new ListTimeSheetCommand().Handle(service);
    }
}
