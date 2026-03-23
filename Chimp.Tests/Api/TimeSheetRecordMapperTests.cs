using System;
using System.Linq;
using AutoBogus;
using Chimp.Api;
using Chimp.Api.Models;
using Shouldly;

namespace Chimp.Tests.Api;

public class TimeSheetRecordMapperTests
{
    [Test]
    public void TestMapToTimeSheetRows()
    {
        var records = new AutoFaker<TimeSheetRecord>()
            .RuleFor(p => p.Start, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .RuleFor(p => p.End, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .Generate(5);

        var result = TimeSheetRecordMapper.MapToTimeSheetRows(records!.ToDictionary(r => r.Id, r => r));

        result.Length.ShouldBe(5);

        var record1 = records.SingleOrDefault(r => r.Id == result[0].Id);
        record1.ShouldNotBeNull("Should be able to find a matching record for the first row in the result.");
        result[0].Id.ShouldBe(record1.Id);
        result[0].ProjectTaskId.ShouldBe(record1.ProjectTaskId);
        result[0].TimeDetails.Date.ShouldBe(DateOnly.FromDateTime(record1.Date));
        result[0].TagIds.ShouldBe(record1.TagIds);
        result[0].Notes.ShouldBe(record1.Notes);
        result[0].TimeDetails.Start!.Value.ToUniversalTime().ShouldBe(record1.Start!.Value);
        result[0].TimeDetails.End!.Value.ToUniversalTime().ShouldBe(record1.End!.Value);
        result[0].TimeDetails.Hours.ShouldBe(record1.Hours);
        result[0].ProjectName.ShouldBe(record1.ProjectName);
        result[0].TaskName.ShouldBe(record1.TaskName);
        result[0].TagNames.ShouldBe(record1.TagNames);
        result[0].Billable.ShouldBe(record1.Billable);

        result.Select(r => r.TimeDetails.Date).ShouldBeInOrder();
        result.Select(r => r.ShortId).ToArray().Select(i => i.Value).ShouldBe([1,2,3,4,5]);
    }
}
