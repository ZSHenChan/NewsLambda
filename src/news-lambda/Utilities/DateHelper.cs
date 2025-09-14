namespace Utilities;

public static class DateHelper
{
  public static DateTime PreviousWeekday(DateTime date)
  {
    do
    {
      date = date.AddDays(-1);
    } while (IsWeekend(date) || IsWeekend(date));

    return date;
  }

  public static DateTime PreviousWeekday(string dateStr)
  {
    DateTime date = DateTime.Parse(dateStr);
    do
    {
      date = date.AddDays(-1);
    } while (IsWeekend(date) || IsWeekend(date));

    return date;
  }

  public static bool IsWeekend(DateTime date)
  {
    return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
  }
}
