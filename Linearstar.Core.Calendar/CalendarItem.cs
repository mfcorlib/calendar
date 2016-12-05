using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Linearstar.Core.Calendar
{
	public class CalendarItem
	{
		public string ComponentKind { get; set; }
		public IDictionary<string, CalendarValue> Properties { get; set; }
		public IList<CalendarItem> Items { get; set; }

		public CalendarItem(string componentKind)
		{
			ComponentKind = componentKind;
			Properties = new Dictionary<string, CalendarValue>();
			Items = new List<CalendarItem>();
		}

		protected virtual CalendarValue ParseValue(string key, IReadOnlyList<string> value, ILookup<string, string> parameters) =>
			CalendarValue.Parse(value, parameters);

		public static CalendarItem Parse(string text)
		{
			using (var lines = SplitLines(text).GetEnumerator())
			{
				lines.MoveNext();

				return Parse(lines);
			}
		}

		static CalendarItem Parse(IEnumerator<string> lines)
		{
			if (!lines.Current.StartsWith("BEGIN:"))
				throw new ArgumentException("not a calendar item text");

			var kind = lines.Current.Substring(6);
			var item =
				kind == CalendarDocument.Kind ? new CalendarDocument() :
				kind == CalendarEvent.Kind ? new CalendarEvent() :
				new CalendarItem(kind);

			while (lines.MoveNext())
			{
				var i = lines.Current;
				var idx = i.IndexOfAny(new[] { ':', ';' });
				var key = i.Substring(0, idx);
				var value = i.Substring(idx + 1);

				switch (key)
				{
					case "BEGIN":
						item.Items.Add(Parse(lines));

						break;
					case "END":
						return item;
					default:
						var values = i.StartsWith(key + ";") ? value.Split(new[] { ':' }, 2) : new[] { value };
						var parameters = values.Take(values.Length - 1).Select(_ => _.Split(new[] { '=' }, 2)).SelectMany(_ => _.Last().Split(',').Select(v => new
						{
							Key = _.First(),
							Value = CalendarValue.UnescapeValueString(v),
						})).ToLookup(_ => _.Key, _ => _.Value);
						var parsed = item.ParseValue(key, values.Last().Split(',').Select(CalendarValue.UnescapeValueString).ToArray(), parameters);

						if (parsed != null)
							item.Properties[key] = parsed;

						break;
				}
			}

			throw new InvalidOperationException("calendar item not properly ended");
		}

		static IEnumerable<string> SplitLines(string text)
		{
			string rt = null;

			foreach (var i in text.Split('\n').Select(_ => _.TrimEnd('\r')))
			{
				if (i.StartsWith(" "))
					rt += i.Substring(1);
				else
				{
					if (!string.IsNullOrEmpty(rt))
						yield return rt;

					rt = i;
				}
			}

			if (!string.IsNullOrEmpty(rt))
				yield return rt;
		}

		protected virtual IEnumerable<Tuple<string, CalendarValue>> WriteValues() =>
			Enumerable.Empty<Tuple<string, CalendarValue>>();

		string PropertiesToString() =>
			string.Join(null, WriteValues().Where(_ => _.Item2.Value != null).Concat(Properties.Select(_ => Tuple.Create(_.Key, _.Value)))
				.Select(_ => _.Item1
					+ (_.Item2.Parameters.Any() ? string.Join(null, _.Item2.Parameters.Select(p => ";" + p.Key + "=" + string.Join(",", p.Value.Select(CalendarValue.EscapeValueString)))) : null)
					+ ":" + string.Join(",", _.Item2.Value.Select(CalendarValue.EscapeValueString))
					+ "\r\n"));

		public override string ToString() =>
			$"BEGIN:{ComponentKind}\r\n" +
			PropertiesToString() +
			string.Join(null, Items ?? Enumerable.Empty<CalendarItem>()) +
			$"END:{ComponentKind}\r\n";

		public static DateTime ParseDateTime(string text) =>
			DateTime.ParseExact(text.TrimEnd('Z'), new[]
			{
				"yyyyMMdd'T'HHmmss",
				"yyyyMMdd"
			}, CultureInfo.InvariantCulture, text.EndsWith("Z") ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal);

		public static string ToDateTimeString(DateTime dateTime, bool includeTime = true) =>
			dateTime.ToString(includeTime ? "yyyyMMdd'T'HHmmss" : "yyyyMMdd") + (includeTime && dateTime.Kind == DateTimeKind.Utc ? "Z" : null);

		public static TimeSpan ParseTimeSpan(string text)
		{
			var rt = TimeSpan.Zero;
			var isNegative = false;
			string numbers = null;

			foreach (var i in text)
				switch (i)
				{
					case '+':
						isNegative = false;

						break;
					case '-':
						isNegative = true;

						break;
					case 'P':
					case 'T':
						break;
					case 'W':
						rt = rt.Add(TimeSpan.FromDays(7 * int.Parse(numbers)));
						numbers = null;

						break;
					case 'D':
						rt = rt.Add(TimeSpan.FromDays(int.Parse(numbers)));
						numbers = null;

						break;
					case 'H':
						rt = rt.Add(TimeSpan.FromHours(int.Parse(numbers)));
						numbers = null;

						break;
					case 'M':
						rt = rt.Add(TimeSpan.FromMinutes(int.Parse(numbers)));
						numbers = null;

						break;
					case 'S':
						rt = rt.Add(TimeSpan.FromSeconds(int.Parse(numbers)));
						numbers = null;

						break;
					default:
						numbers += i;

						break;
				}

			if (isNegative)
				rt = -rt;

			return rt;
		}

		public static string ToTimeSpanString(TimeSpan timeSpan) =>
			"P" + (timeSpan.Days >= 7 ? (timeSpan.Days / 7) + "W" : null)
				+ (timeSpan.Days > 0 ? (timeSpan.Days % 7) + "D" : null)
				+ (timeSpan.Hours > 0 || timeSpan.Minutes > 0 || timeSpan.Seconds > 0 ? "T" : null)
				+ (timeSpan.Hours > 0 ? timeSpan.Hours + "H" : null)
				+ (timeSpan.Minutes > 0 ? timeSpan.Minutes + "M" : null)
				+ (timeSpan.Seconds > 0 ? timeSpan.Seconds + "S" : null);
	}
}
