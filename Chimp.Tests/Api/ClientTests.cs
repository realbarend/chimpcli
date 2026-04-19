using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoBogus;
using Chimp.Api;
using Chimp.Api.Models;
using Chimp.Common;
using Chimp.DomainModels;
using Moq;
using Shouldly;

namespace Chimp.Tests.Api;

public class ClientTests
{
    private PersistablePropertyBag _state = null!;
    private HttpMessageHandlerMocker _requestMocker = null!;
    private Client _sut = null!;

    [SetUp]
    public void Setup()
    {
        _state = new();
        _requestMocker = new();
        var mockTokenProvider = new Mock<ICognitoAuthentication>();
        mockTokenProvider.Setup(tp => tp.GetValidBearerToken()).ReturnsAsync("FAKE_ACCESS_TOKEN");
        _sut = new Client(_state, mockTokenProvider.Object, new HttpClient(_requestMocker.Mock.Object) { BaseAddress = new Uri("http://unit.test/api/") }, new DebugLogger(new Mock<TextWriter>().Object));
    }

    [Test]
    public async Task TestThatTagsAreReadFromCache()
    {
        var cachedTags = new AutoFaker<Tag>().Generate(5).ToArray();
        _state.Set(cachedTags);

        var tags = await _sut.GetTags();
        tags.ShouldBe(cachedTags);
    }

    [Test]
    public async Task TestThatTagsAreFetchedFromApiAndMappedCorrectly()
    {
        _requestMocker.AddRequest(HttpRequestMock.ForRequestPath("/api/tag/type/1", new[] { new TagRecord { CompanyId = 123, Id = 12, Name = "tagname", Active = true, Type = 1 } }));
        var tags = await _sut.GetTags();

        tags.ShouldBe([new Tag { ShortId = new ShortId<Tag>(1), Id = 12, Name = "tagname" }]);
    }

    [Test]
    public async Task TestThatAddTimeSheetRowMapsDataCorrectlyInBothWays()
    {
        long userId = 12113, projectId = 123, projectTaskId = 234, customerId = 1234, recordId = 555;
        _state.Set(new AutoFaker<UserRecord>().RuleFor(p => p.Id, _ => userId).Generate());

        var inputDto = new TimeSheetRowDto
        {
            ProjectTaskId = projectTaskId,
            TimeDetails = new TimeDetails
            {
                Date = new DateOnly(2025, 11, 10),
                Start = new DateTime(2025, 11, 10, 9, 15, 0, DateTimeKind.Local),
                End = new DateTime(2025, 11, 10, 10, 45, 0, DateTimeKind.Local),
                Hours = 1.5,
            },
            Notes = "a test note",
            TagIds = [101, 102],
        };

        var expectedFakeResultRecord = new AutoFaker<TimeSheetRecord>()
            .RuleFor(p => p.Id, _ => recordId)
            .RuleFor(p => p.Start, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .RuleFor(p => p.End, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .Generate();
        var addRowRequest = HttpRequestMock.ForRequestPath("/api/time", expectedFakeResultRecord);
        _requestMocker
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/project/{userId}/uiselectbyuser", new[] { new ProjectRecord { Id = projectId, CustomerId = customerId, Name = "project", Intern = false } } ))
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/projecttask/uiselect/project/{projectId}", new[] { new ProjectTaskRecord { Id = projectTaskId, Name = "projecttask" } } ))
            .AddRequest(addRowRequest)
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/time/week/{userId}/2025-11-10", new[] { expectedFakeResultRecord }));
        addRowRequest.Assertions.Add(r =>
        {
            // Here we verify that the input is correctly mapped to the api request.
            var obj = JsonSerializer.Deserialize<NewTimeSheetRecord>(r.Content?.ReadAsStringAsync().Result ?? "", new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase})!;
            obj.ShouldBeEquivalentTo(new NewTimeSheetRecord
            {
                CustomerId = customerId,
                ProjectId = projectId,
                ProjectTaskId = projectTaskId,
                Date = inputDto.TimeDetails.Date.ToDateTime(TimeOnly.MinValue),
                Hours = 1.5,
                Notes = inputDto.Notes,
                Start = inputDto.TimeDetails.Start!.Value.ToUniversalTime(),
                End = inputDto.TimeDetails.End!.Value.ToUniversalTime(),
                StartEnd = "09:15-10:45",
                TagIds = inputDto.TagIds,
                Timezone = TimeSheetRecordMapper.GetCurrentTimeZoneIanaName(),
            });
            obj.Start!.Value.Kind.ShouldBe(DateTimeKind.Utc);
            obj.End!.Value.Kind.ShouldBe(DateTimeKind.Utc);
        });

        var result = await _sut.AddTimeSheetRow(inputDto);

        // Sut does not return data directly from the fed dto, but from a record returned by the API.
        // So the row properties are not expected to match our dto, but a mapped version of the fake record.
        result.ShortId.ShouldBe(new ShortId<TimeSheetRow>(1));
        result.Id.ShouldBe(recordId);
   }

    [Test]
    public async Task TestUpdateTimeSheetRowMapsDataCorrectly()
    {
        long userId = 12113, projectId = 123, projectTaskId = 234, customerId = 1234, recordId = 555;
        _state.Set(new AutoFaker<UserRecord>().RuleFor(p => p.Id, _ => userId).Generate());

        var inputDto = new TimeSheetRowDto
        {
            ProjectTaskId = projectTaskId,
            TimeDetails = new TimeDetails
            {
                Date = new DateOnly(2025, 11, 10),
                Start = new DateTime(2025, 11, 10, 9, 15, 0, DateTimeKind.Local),
                End = new DateTime(2025, 11, 10, 10, 45, 0, DateTimeKind.Local),
                Hours = 1.5,
            },
            Notes = "a test note",
            TagIds = [101, 102],
        };

        var fakeOriginalRecord = new AutoFaker<TimeSheetRecord>()
            .RuleFor(p => p.Id, _ => recordId)
            .RuleFor(p => p.Start, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .RuleFor(p => p.End, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Unspecified))
            .Generate();

        var updateRowRequest = HttpRequestMock.ForRequestPath($"/api/time/{recordId}", fakeOriginalRecord);
        _requestMocker
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/time/week/{userId}/2025-11-10", new[] { fakeOriginalRecord }))
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/project/{userId}/uiselectbyuser", new[] { new ProjectRecord { Id = projectId, CustomerId = customerId, Name = "project", Intern = false } } ))
            .AddRequest(HttpRequestMock.ForRequestPath($"/api/projecttask/uiselect/project/{projectId}", new[] { new ProjectTaskRecord { Id = projectTaskId, Name = "projecttask" } } ))
            .AddRequest(updateRowRequest);
        updateRowRequest.Assertions.Add(r =>
        {
            // Here we verify that the input is correctly mapped to the api request.
            var obj = JsonSerializer.Deserialize<TimeSheetRecord>(r.Content?.ReadAsStringAsync().Result ?? "", new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase})!;
            obj.ShouldBeEquivalentTo(fakeOriginalRecord with
            {
                CustomerId = customerId,
                ProjectId = projectId,
                ProjectTaskId = projectTaskId,
                Date = inputDto.TimeDetails.Date.ToDateTime(TimeOnly.MinValue),
                Hours = 1.5,
                Notes = inputDto.Notes,
                Start = inputDto.TimeDetails.Start!.Value.ToUniversalTime(),
                End = inputDto.TimeDetails.End!.Value.ToUniversalTime(),
                StartEnd = "09:15-10:45",
                TagIds = inputDto.TagIds,
                Timezone = TimeSheetRecordMapper.GetCurrentTimeZoneIanaName(),
            });
            obj.Start!.Value.Kind.ShouldBe(DateTimeKind.Utc);
            obj.End!.Value.Kind.ShouldBe(DateTimeKind.Utc);
        });

        var result = await _sut.UpdateTimeSheetRow(recordId, inputDto);

        // Sut does not return data directly from fed dto, but from a record returned by the API.
        // So the row properties are not expected to match our dto, but a mapped version of the fake record.
        result.ShortId.ShouldBe(new ShortId<TimeSheetRow>(1));
        result.Id.ShouldBe(recordId);
    }
}
