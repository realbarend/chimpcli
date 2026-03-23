using System.Diagnostics;
using Chimp.Api.Models;
using Chimp.DomainModels;

namespace Chimp.Api;

public static class TimeSheetRecordMapper
{
    public static TimeSheetRow[] MapToTimeSheetRows(Dictionary<long, TimeSheetRecord> records)
        => records.Values.OrderBy(r => r.Date).ThenBy(d => !d.End.HasValue).ThenBy(r => r.Start).Select((r, i) => MapToRow(r, i + 1)).ToArray();

    private static TimeSheetRow MapToRow(TimeSheetRecord record, int shortId)
    {
        // The server sends all date fields as yyyy-MM-ddTHH:mm:ss, which does _not_ include timezone information.
        // However, these dates seem to be are _always_ in UTC.
        // The date _string_ fields such as StartEnd are in _local_ time, probably using the 'Timezone' field as an indicator.

        // Because the fields lack timezone information, the resulting DateTime instances have 'Unspecified' kind.
        // Below we use 'ToLocalTime', which treats unspecified-kind dates as 'UTC', which is exactly what we need.

        // Additionally: The 'Date' field is always sent as yyyy-MM-ddT00:00:00, so with 0:00 time part.
        // We should never try to convert that to 'local' time, because 'Date' really indicates a specific date.
        // Hence we convert that to DateOnly.

        var row = new TimeSheetRow
        {
            ShortId = new ShortId<TimeSheetRow>(shortId),
            Id = record.Id,
            ProjectTaskId = record.ProjectTaskId,

            TimeDetails = new TimeDetails
            {
                Date = new DateOnly(record.Date.Year, record.Date.Month, record.Date.Day),
                Start = record.Start?.ToLocalTime(),
                End = record.End?.ToLocalTime(),
                Hours = record.Hours,
            },

            TagIds = record.TagIds ?? [],
            Notes = record.Notes,

            ProjectName = record.ProjectName,
            TaskName = record.TaskName,
            TagNames = record.TagNames ?? [],
            Billable = record.Billable,
        };
        return row;
    }

    public static NewTimeSheetRecord MapToNewRecord(TimeSheetRowDto dto, ProjectTask? project)
    {
        dto.TimeDetails.Validate();
        Debug.Assert(project != null, "should be able to find project for new row");

        var obj = new NewTimeSheetRecord
        {
            // UserId will be added serverside

            // required: define the customer, project and project task
            ProjectTaskId = dto.ProjectTaskId,
            CustomerId = project.CustomerId,
            ProjectId = project.ProjectId,
            // TaskId will be added serverside

            // required: define the calendar date
            Date = MapToRecord(dto.TimeDetails.Date),

            // Hours defines the amount of hours worked: optional.
            // If Hours is empty and start-end are specified, then the server will calculate and set Hours automatically.
            // If Hours and start-end are both specified, then they do not need to match (!),
            // so it is allowed to specify hours and start-end separately.
            Hours = dto.TimeDetails.Hours,
            TagIds = dto.TagIds,
            Notes = dto.Notes, // optional

            // Start, End and StartEnd are optional, but when used, all three must be specified.
            Start = MapToRecord(dto.TimeDetails.Start),
            End = MapToRecord(dto.TimeDetails.End),
            StartEnd = MapToStartEnd(dto.TimeDetails.Start, dto.TimeDetails.End),
            // Timezone is _not_ used for date calculations, but may be used to display the dates in the correct timezone.
            Timezone = GetCurrentTimeZoneIanaName(),
        };

        return obj;
    }

    public static TimeSheetRecord MapToUpdateRecord(TimeSheetRecord record, TimeSheetRowDto dto, ProjectTask? project)
    {
        dto.TimeDetails.Validate();

        // Careful: when updating a timesheet row but not the project, we may not have access to a related project record.
        // In that case, we need to use the customerId and projectId from the existing record.
        var customerId = record.CustomerId;
        var projectId = record.ProjectId;
        var projectTaskId = record.ProjectTaskId;
        if (record.ProjectTaskId != dto.ProjectTaskId)
        {
            Debug.Assert(project != null, "should be able to find project for updated project");
            customerId = project.CustomerId;
            projectId = project.ProjectId;
            projectTaskId = project.Id;
        }

        return record with
        {
            CustomerId = customerId,
            ProjectId = projectId,
            ProjectTaskId = projectTaskId,
            // TaskId and other project related fields will get updated serverside

            Date = MapToRecord(dto.TimeDetails.Date),
            Hours = dto.TimeDetails.Hours,
            TagIds = dto.TagIds,
            Notes = dto.Notes,

            Start = MapToRecord(dto.TimeDetails.Start),
            End = MapToRecord(dto.TimeDetails.End),
            StartEnd = MapToStartEnd(dto.TimeDetails.Start, dto.TimeDetails.End),
            Timezone = GetCurrentTimeZoneIanaName(),
        };
    }

    public static string GetCurrentTimeZoneIanaName()
    {
        if (TimeZoneInfo.Local.HasIanaId) return TimeZoneInfo.Local.Id;
        return TimeZoneInfo.TryConvertWindowsIdToIanaId(TimeZoneInfo.Local.Id, out var id1) ? id1 : TimeZoneInfo.Local.Id;
    }

    private static DateTime MapToRecord(DateOnly rowDate) => DateTime.SpecifyKind(rowDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    private static DateTime? MapToRecord(DateTime? dateTime) => dateTime?.ToUniversalTime();

    private static string? MapToStartEnd(DateTime? start, DateTime? end)
    {
        if (!start.HasValue || !end.HasValue) return null;
        return $"{start.Value:HH:mm}-{end.Value:HH:mm}";
    }
}
