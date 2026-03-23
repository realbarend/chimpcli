using System;
using System.IO;
using System.Threading.Tasks;
using AutoBogus;
using Chimp.Api;
using Chimp.Common;
using Chimp.DomainModels;
using Chimp.Services;
using Moq;
using Shouldly;

namespace Chimp.Tests.Services;

[TestFixture]
public class TimeSheetServiceTests
{
    [Test]
    public async Task TestAddTimeSheetRowCorrectlyMapsProperties()
    {
        var mockClient = new Mock<IClient>();

        var sut = new TimeSheetService(new PersistablePropertyBag(), mockClient.Object, new DebugLogger(new Mock<TextWriter>().Object));
        var projectTask = new AutoFaker<ShortId<ProjectTask>>().Generate();
        var tags = new AutoFaker<ShortId<Tag>>().Generate(2).ToArray();
        var timeEntry = new TimeEntry { StartTime = new TimeOnly(12, 30), Duration = TimeSpan.FromHours(1), DayOfWeek = DayOfWeek.Tuesday };
        var timeSheet = await sut.GetTimeSheet();
        var time = new AutoFaker<TimeDetails>()
            .RuleFor(p => p.Date, _ => DateOnly.FromDateTime(DateTime.Today))
            .Generate();
        var returnRow = new AutoFaker<TimeSheetRow>()
            .RuleFor(p => p.TimeDetails, _ => time)
            .Generate();

        mockClient.Setup(client => client.GetProjectTasks())
            .ReturnsAsync([new ProjectTask {  ShortId = projectTask, Id = 123, CustomerId = 1, ProjectId = 1, ProjectName = "project", TaskName = "task" }]);
        mockClient.Setup(client => client.GetTags())
            .ReturnsAsync([new Tag { ShortId = tags[0], Id = 1, Name =  "test-tag-1" }, new Tag { ShortId = tags[1], Id = 2, Name =  "test-tag-2" }]);
        mockClient.Setup(client => client.GetTimeSheetRows(It.IsAny<DateOnly>()))
            .ReturnsAsync([]);
        mockClient.Setup(client => client.AddTimeSheetRow(It.IsAny<TimeSheetRowDto>()))
            .ReturnsAsync(returnRow)
            .Callback<TimeSheetRowDto>(dto =>
            {
                // test that all properties got mapped to the dto
                dto.ProjectTaskId.ShouldBe(123);
                dto.TimeDetails.ShouldBe(TimeDetails.FromTimeEntry(timeEntry, timeSheet));
                dto.TagIds.ShouldBe([1,2]);
                dto.Notes.ShouldBe("test notes");
            });

        var result = await sut.AddTimeSheetRow(projectTask, tags, timeEntry, "test notes");
        result.ShouldBe(returnRow);
    }

    [Test]
    public async Task TestUpdateTimeSheetRowCorrectlyMapsPropertiesWhenNoPropertiesGotChanged()
    {
        var mockClient = new Mock<IClient>();

        var sut = new TimeSheetService(new PersistablePropertyBag(), mockClient.Object, new DebugLogger(new Mock<TextWriter>().Object));
        var time = new AutoFaker<TimeDetails>()
            .RuleFor(p => p.Date, _ => DateOnly.FromDateTime(DateTime.Today))
            .Generate();
        var row = new AutoFaker<TimeSheetRow>()
            .RuleFor(p => p.TimeDetails, _ => time)
            .Generate();

        mockClient.Setup(client => client.GetProjectTasks())
            .ReturnsAsync([]);
        mockClient.Setup(client => client.GetTags())
            .ReturnsAsync([]);
        mockClient.Setup(client => client.GetTimeSheetRows(It.IsAny<DateOnly>()))
            .ReturnsAsync([row]);
        mockClient.Setup(client => client.UpdateTimeSheetRow(It.IsAny<long>(), It.IsAny<TimeSheetRowDto>()))
            .ReturnsAsync(row)
            .Callback<long, TimeSheetRowDto>((id, dto) =>
            {
                id.ShouldBe(row.Id);

                // test that all properties got mapped to the dto
                dto.ProjectTaskId.ShouldBe(row.ProjectTaskId);
                dto.TimeDetails.ShouldBe(row.TimeDetails);
                dto.TagIds.ShouldBe(row.TagIds);
                dto.Notes.ShouldBe(row.Notes);
            });

        var result = await sut.UpdateTimeSheetRow(row.ShortId, null, null, null, null);
        result.ShouldBe(row);
    }

    [Test]
    public async Task TestUpdateTimeSheetRowCorrectlyMapsPropertiesWhenAllPropertiesGotChanged()
    {
        var mockClient = new Mock<IClient>();
        var sut = new TimeSheetService(new PersistablePropertyBag(), mockClient.Object, new DebugLogger(new Mock<TextWriter>().Object));
        var timeSheet = await sut.GetTimeSheet();

        // new data
        var projectTask = new AutoFaker<ShortId<ProjectTask>>().Generate();
        var tags = new AutoFaker<ShortId<Tag>>().Generate(2).ToArray();
        var timeEntry = new TimeEntry { StartTime = new TimeOnly(12, 30), Duration = TimeSpan.FromHours(1), DayOfWeek = DayOfWeek.Tuesday };
        var notes = "new notes";

        var time = new AutoFaker<TimeDetails>()
            .RuleFor(p => p.Date, _ => DateOnly.FromDateTime(DateTime.Today))
            .Generate();
        var row = new AutoFaker<TimeSheetRow>()
            .RuleFor(p => p.TimeDetails, _ => time)
            .Generate();

        mockClient.Setup(client => client.GetProjectTasks())
            .ReturnsAsync([new ProjectTask {  ShortId = projectTask, Id = 123, CustomerId = 1, ProjectId = 1, ProjectName = "project", TaskName = "task" }]);
        mockClient.Setup(client => client.GetTags())
            .ReturnsAsync([new Tag { ShortId = tags[0], Id = 1, Name =  "test-tag-1" }, new Tag { ShortId = tags[1], Id = 2, Name =  "test-tag-2" }]);
        mockClient.Setup(client => client.GetTimeSheetRows(It.IsAny<DateOnly>()))
            .ReturnsAsync([row]);
        mockClient.Setup(client => client.UpdateTimeSheetRow(It.IsAny<long>(), It.IsAny<TimeSheetRowDto>()))
            .ReturnsAsync(row)
            .Callback<long, TimeSheetRowDto>((id, dto) =>
            {
                id.ShouldBe(row.Id);

                // test that all properties got mapped to the dto
                dto.ProjectTaskId.ShouldBe(123);
                dto.TimeDetails.ShouldBe(TimeDetails.FromTimeEntry(timeEntry, timeSheet));
                dto.TagIds.ShouldBe([1,2]);
                dto.Notes.ShouldBe(notes);
            });

        var result = await sut.UpdateTimeSheetRow(row.ShortId, projectTask, tags, timeEntry, notes);
        result.ShouldBe(row);
    }


    [Test]
    public async Task TestCopyTimeSheetRowCorrectlyMapsProperties()
    {
        var mockClient = new Mock<IClient>();
        var sut = new TimeSheetService(new PersistablePropertyBag(), mockClient.Object, new DebugLogger(new Mock<TextWriter>().Object));
        var timeSheet = await sut.GetTimeSheet();

        var projectTask = new AutoFaker<ShortId<ProjectTask>>().Generate();
        var tags = new AutoFaker<ShortId<Tag>>().Generate(2).ToArray();
        var timeEntry = new TimeEntry { StartTime = new TimeOnly(12, 30), Duration = TimeSpan.FromHours(1), DayOfWeek = DayOfWeek.Tuesday };

        var time = new AutoFaker<TimeDetails>()
            .RuleFor(p => p.Date, _ => DateOnly.FromDateTime(DateTime.Today))
            .Generate();
        var row = new AutoFaker<TimeSheetRow>()
            .RuleFor(p => p.TimeDetails, _ => time)
            .Generate();
        row = row with { ProjectTaskId = 123, TagIds = [1,2] };

        mockClient.Setup(client => client.GetProjectTasks())
            .ReturnsAsync([new ProjectTask {  ShortId = projectTask, Id = 123, CustomerId = 1, ProjectId = 1, ProjectName = "project", TaskName = "task" }]);
        mockClient.Setup(client => client.GetTags())
            .ReturnsAsync([new Tag { ShortId = tags[0], Id = 1, Name =  "test-tag-1" }, new Tag { ShortId = tags[1], Id = 2, Name =  "test-tag-2" }]);
        mockClient.Setup(client => client.GetTimeSheetRows(It.IsAny<DateOnly>()))
            .ReturnsAsync([row]);
        mockClient.Setup(client => client.AddTimeSheetRow(It.IsAny<TimeSheetRowDto>()))
            .ReturnsAsync(row)
            .Callback<TimeSheetRowDto>(dto =>
            {
                // the time details for the new row, should match our new timeEntry
                dto.TimeDetails.ShouldBe(TimeDetails.FromTimeEntry(timeEntry, timeSheet));
                // other properties should match the original row
                dto.ProjectTaskId.ShouldBe(123);
                dto.TagIds.ShouldBe([1,2]);
                dto.Notes.ShouldBe(row.Notes);
            });

        var result = await sut.CopyTimeSheetRow(row.ShortId, timeEntry);
        result.ShouldBe(row);
    }
}
