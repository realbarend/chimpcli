using Chimp.DomainModels;
using Chimp.Services;

namespace Chimp.Shell;

public class UpdateTimeSheetRowCommand : IShellCommand
{
    public required ShortId<TimeSheetRow> ShortId { get; init; }
    public ShortId<ProjectTask>? Project  { get; init; }
    public ShortId<Tag>[]? Tags { get; init; }
    public TimeEntry? TimeEntry { get; init; }
    public string? Notes { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        await service.UpdateTimeSheetRow(ShortId, Project, Tags, TimeEntry, Notes);

        await new ListTimeSheetCommand().Handle(service);
    }
}
