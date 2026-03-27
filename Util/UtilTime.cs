

using System;

namespace KHFC {
	internal partial class Util {
		public readonly static DateTime epochTime = new(1970, 1, 1);

		/// <summary> UTC 기준 현재 시각 </summary>
		public static int utcNow => (int)(DateTime.UtcNow - epochTime).TotalSeconds;
		/// <summary> UTC 기준 오늘 0시 </summary>
		public static int utcToday => (int)(DateTime.UtcNow.Date - epochTime).TotalSeconds;
		/// <summary> UTC 기준 내일 0시 </summary>
		public static int utcTomorrow => (int)(DateTime.UtcNow.Date - epochTime).TotalSeconds + 86400;
		/// <summary> UTC 기준 이번 주 월요일 0시, 주 시작은 일요일 기준임 </summary>
		public static int utcCurMonday {
			get {
				DateTime dateToday = DateTime.Today;
				DateTime dateMonday = dateToday.AddDays(Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(dateToday.DayOfWeek));
				return (int)((dateMonday.Ticks - epochTime.Ticks) / TimeSpan.TicksPerSecond);
			}
		}
		/// <summary> UTC 기준 다음 주 월요일 0시, 주 시작은 일요일 기준임 </summary>
		public static int utcNextMonday => utcCurMonday + (86400 * 7);
		/// <summary> UTC 기준 다음 달 1일 0시 </summary>
		public static int utcNextMonth {
			get {
				DateTime dateToday = DateTime.Today;
				DateTime NextMonth = new DateTime(dateToday.Year, dateToday.Month, 1, 0, 0, 0).AddMonths(1);
				return (int)((NextMonth.Ticks - epochTime.Ticks) / TimeSpan.TicksPerSecond);
			}
		}

		/// <summary> 올해 첫 월요일을 기준으로 오늘의 주차를 리턴, 첫 월요일 주차의 값은 1이다 -> 항상 1보다 큰값을 리턴함 </summary>
		public static int weekOfToday => GetWeeks(DateTime.Today);


		//public static int GetUTCTodayZero() {
		//	DateTime today = DateTime.UtcNow.Date;
		//	return (int)((today.Ticks - epochTime.Ticks) / TimeSpan.TicksPerSecond);  //UTC기준 오늘 0시
		//}

		/// <summary> 올해 첫 월요일을 기준으로 <paramref name="time"/>의 주차를 리턴, 첫 월요일 주차의 값은 1이다 -> 항상 1보다 큰값을 리턴함 </summary>
		public static int GetWeeksFromTimestamp(int time) {
			return GetWeeks(ToDateTime(time));
		}
		/// <summary> 올해 첫 월요일을 기준으로 <paramref name="day"/>의 주차를 리턴, 첫 월요일 주차의 값은 1이다 -> 항상 1보다 큰값을 리턴함 </summary>
		public static int GetWeeks(DateTime day) {
			DateTime firstDay = new(day.Year, 1, 1, 0, 0, 0);
			int firstMonday = ((int)firstDay.DayOfWeek + 7) % 7;

			// 오늘이 올해 첫 월요일보다 빠른 날이면 현재 주는 작년의 연장선이다
			if (day.Month == 1 && day.Day < firstMonday) {
				firstDay = new(day.Year - 1, 1, 1, 0, 0, 0);
				firstMonday = ((int)firstDay.DayOfWeek + 7) % 7;
			}

			return ((int)(day - firstDay).TotalDays - (firstMonday - 1)) / 7 + 1;
		}
		public static DateTime GetFirstMonday(int year) {
			DateTime firstDay = new(year, 1, 1, 0, 0, 0);
			return firstDay.AddDays(((int)firstDay.DayOfWeek + 7) % 7 - 1);
		}

		public static int GetUTCDateZero(int time) {
			DateTime date = epochTime.AddSeconds(time);
			return (int)((date.Ticks - epochTime.Ticks) / TimeSpan.TicksPerSecond - date.TimeOfDay.TotalSeconds);
		}

		/// <summary> TimeStamp를 <see cref="DateTime"/> 으로 변환 </summary>
		public static DateTime ToDateTime(int time) {
			return epochTime.AddSeconds(time);
		}

		/// <summary> TimeStamp를 string 으로 변환 </summary>
		public static string ToString(double time) {
			DateTime date = epochTime.AddSeconds(time);
			return date.ToString("yyyyMMddHHmmssfff");
		}

		/// <summary> TimeStamp를 string 으로 변환 </summary>
		public static string ToInDateFormat(double time) {
			DateTime date = epochTime.AddSeconds(time);
			return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
		}

		/// <summary> TimeStamp 형식의 string을 TimeStamp 으로 변환 </summary>
		public static double ToTimestamp(string time) {
			TimeSpan timeSpan = (DateTime.Parse(time) - epochTime);
			return timeSpan.TotalSeconds;
		}

		/// <summary> TimeStamp 형식의 string을 TimeStamp 으로 변환 </summary>
		public static double ToTimestampUTC(string time) {
			DateTime parsedUTCDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(time));
			TimeSpan timeSpan = (parsedUTCDate - epochTime);
			return timeSpan.TotalSeconds;
		}
	}
}