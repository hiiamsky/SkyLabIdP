namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface IDateService
    {
        DateTime Now { get; }

        DateTime? TryParseDate(string dateString);
        string ConvertToTaiwanCalendar(DateTime? date);

        string ConvertToSimpleTaiwanCalendar(DateTime? date);

        string ConvertToTaiwanCalendarByOpenData(DateTime? date);

        string ConvertToGregorianCalendar(string taiwanDate);

        string DateFormatByOpenData(DateTime? date);
    }
}

