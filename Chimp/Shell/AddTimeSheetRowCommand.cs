using Chimp.DomainModels;
using Chimp.Services;

namespace Chimp.Shell;

public class AddTimeSheetRowCommand : IShellCommand
{
    public required ShortId<ProjectTask> Project  { get; init; }
    public required ShortId<Tag>[] Tags { get; init; }
    public required TimeEntry TimeEntry { get; init; }
    public required string Notes { get; init; }

    public async Task Handle(TimeSheetService service)
    {
        await service.AddTimeSheetRow(Project, Tags, TimeEntry, Notes);

        await new ListTimeSheetCommand().Handle(service);
    }
}
