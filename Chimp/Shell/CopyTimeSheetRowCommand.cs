using Chimp.DomainModels;
using Chimp.Services;

namespace Chimp.Shell;

public class CopyTimeSheetRowCommand : IShellCommand
{
    public required ShortId<TimeSheetRow> ShortId { get; init; }
    public required TimeEntry TimeEntry { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        await service.CopyTimeSheetRow(ShortId, TimeEntry);

        await new ListTimeSheetCommand().Handle(service);
    }
}
