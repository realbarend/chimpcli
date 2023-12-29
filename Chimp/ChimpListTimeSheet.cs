using Chimp.Models;

namespace Chimp;

public class ChimpListTimeSheet(ChimpService service)
{
    public async Task Run(int? weekOffset = null)
    {
        var localizer = service.GetLocalizer();
        var (weekTotal, billableTotal, rows) = await service.GetTimeSheet(weekOffset);
        var timeTravelerBaseDate = service.GetTimeTravelingDate();
        var isTimeTraveling = timeTravelerBaseDate != null;

        foreach (var date in rows.GroupBy(r => r.Date.Date).Select((a, _) => a.Key).OrderBy(d => d))
        {
            var displayDay = date == DateTime.Today
                ? localizer.GetToday()
                : isTimeTraveling
                    ? localizer.GetLongDate(date)
                    : localizer.GetWeekDay(date); 
            Console.WriteLine();
            Console.WriteLine($"{displayDay.ToUpperInvariant()} =============================================================================");

            var previousRow = (TimeSheetRowViewModel?)null;
            var dayRows = rows.Where(r => r.Date.Date == date).ToList();
            foreach (var row in dayRows.OrderBy(d => d.Start))
            {
                if (previousRow != null && previousRow.End != row.Start) Console.WriteLine("--gap--");
                Console.WriteLine(localizer.GetTimeSheetRow(row));
                previousRow = row;
            }

            Console.WriteLine("{0,-60} {1}", "--", "--");
            Console.WriteLine(localizer.GetDaySummary(weekTotal, billableTotal, displayDay, dayRows.Sum(r => r.Hours)));
        }
        
        if (timeTravelerBaseDate != null)
        {
            var objectiveTime = Util.GetFirstDayOfWeek(DateTime.Now);
            var userTime = Util.GetFirstDayOfWeek(timeTravelerBaseDate.Value);
            
            Console.WriteLine();
            Console.WriteLine(localizer.GetTimeTravelerAlert(Convert.ToInt32((userTime - objectiveTime).TotalDays / 7)));
        }
    }
}
