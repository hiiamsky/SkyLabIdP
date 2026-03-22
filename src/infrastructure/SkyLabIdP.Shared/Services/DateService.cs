using SkyLabIdP.Application.Common.Interfaces;
using System.Globalization;

namespace SkyLabIdP.Shared.Services
{
    public class DateService : IDateService
    {
        private static readonly string Format = "yyyy-MM-dd";
        private static readonly CultureInfo TaiwanCulture = new CultureInfo("zh-TW");

        public DateTime Now => DateTime.UtcNow;

        public DateTime? TryParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, Format, TaiwanCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }

        public string ConvertToTaiwanCalendar(DateTime? date)
        {

            return FormatTaiwanDate(date, "民國{0}年{1}月{2}日");
        }

        public string ConvertToSimpleTaiwanCalendar(DateTime? date)
        {
            return FormatTaiwanDate(date, "{0}/{1:D2}/{2:D2}");
        }

        public string ConvertToTaiwanCalendarByOpenData(DateTime? date)
        {
            return FormatTaiwanDate(date, "{0:D3}{1:D2}{2:D2}");
        }

        public string DateFormatByOpenData(DateTime? date)
        {
            if (date == null) return string.Empty;

            string openDataDate = ((DateTime)date).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return openDataDate;
        }

        public string ConvertToGregorianCalendar(string? taiwanDate)
        {
            // 檢查輸入是否為 null 或空字串
            if (string.IsNullOrEmpty(taiwanDate) || taiwanDate.Length != 7)
            {
                return string.Empty;
            }

            // 解析民國年部分（前3位數），轉換為西元年
            if (int.TryParse(taiwanDate.AsSpan(0, 3), out int taiwanYear))
            {
                int gregorianYear = taiwanYear + 1911;
                string gregorianDate = $"{gregorianYear}-{taiwanDate.Substring(3, 2)}-{taiwanDate.Substring(5, 2)}";

                if (DateTime.TryParseExact(gregorianDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            return string.Empty;
        }
        private static string FormatTaiwanDate(DateTime? date, string format)
        {
            if (date == null) return string.Empty;
            var conversDate = ((DateTime)date);
            TaiwanCalendar taiwanCalendar = new();
            return string.Format(format,
                                 taiwanCalendar.GetYear(conversDate),
                                 taiwanCalendar.GetMonth(conversDate),
                                 taiwanCalendar.GetDayOfMonth(conversDate));
        }
    }
}
