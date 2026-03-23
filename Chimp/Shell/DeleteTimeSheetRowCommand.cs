using Chimp.DomainModels;
using Chimp.Services;

namespace Chimp.Shell;
using static Localization;
using static RenderHelper;

public class DeleteTimeSheetRowCommand : IShellCommand
{
    public required ShortId<TimeSheetRow> ShortId { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        var timeSheet = await service.GetTimeSheet();
        var row = timeSheet.GetRow(ShortId);
        RenderTimeSheetRow(timeSheet, row);

        WriteLocalized("About to delete row #{Line}: are you sure? Y/N", new { Line = ShortId });
        if ( ! ReadLocalizedYesKey())
        {
            WriteLocalized("Not removed.");
            return;
        }

        await service.DeleteRow(row);
        await new ListTimeSheetCommand().Handle(service);
    }
}
