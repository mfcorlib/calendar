using System;
using System.Collections.Generic;
using System.Linq;

namespace Linearstar.Core.Calendar
{
	public class CalendarEvent : CalendarItem
	{
		public const string Kind = "VEVENT";

		public DateTime Start { get; set; }
		public CalendarDateTimeKind StartKind { get; set; }
		public DateTime End { get; set; }
		public CalendarDateTimeKind EndKind { get; set; }
		public DateTime? TimeStamp { get; set; }
		public TimeSpan? Duration { get; set; }
		public string Summary { get; set; }
		public string Description { get; set; }
		public string Location { get; set; }
		public IList<string> Categories { get; set; }
		public CalendarTransparency? Transparency { get; set; }

		public CalendarEvent()
			: base(Kind)
		{
		}

		protected override CalendarValue ParseValue(string key, IReadOnlyList<string> value, ILookup<string, string> parameters)
		{
			switch (key)
			{
				case "DTSTART":
					Start = ParseDateTime(value.First());
					StartKind = parameters["VALUE"].First() == "DATE" ? CalendarDateTimeKind.Date : CalendarDateTimeKind.DateTime;

					return null;
				case "DTEND":
					End = ParseDateTime(value.First());
					EndKind = parameters["VALUE"].First() == "DATE" ? CalendarDateTimeKind.Date : CalendarDateTimeKind.DateTime;

					return null;
				case "DTSTAMP":
					TimeStamp = ParseDateTime(value.First());

					return null;
				case "DURATION":
					Duration = ParseTimeSpan(value.First());

					return null;
				case "SUMMARY":
					Summary = value.First();

					return null;
				case "DESCRIPTION":
					Description = value.First();

					return null;
				case "LOCATION":
					Location = value.First();

					return null;
				case "CATEGORIES":
					Categories = value.ToList();

					return null;
				case "TRANSPARENCY":
					Transparency = value.First() == "TRANSP" ? CalendarTransparency.Transparent : CalendarTransparency.Opaque;

					return null;
				default:
					return base.ParseValue(key, value, parameters);
			}
		}

		protected override IEnumerable<Tuple<string, CalendarValue>> WriteValues()
		{
			yield return Tuple.Create("DTSTART", new CalendarValue(ToDateTimeString(Start, StartKind == CalendarDateTimeKind.DateTime))
			{
				Parameters = {["VALUE"] = new[] { StartKind == CalendarDateTimeKind.DateTime ? "DATE-TIME" : "DATE" } },
			});
			yield return Tuple.Create("DTEND", new CalendarValue(ToDateTimeString(End, EndKind == CalendarDateTimeKind.DateTime))
			{
				Parameters = {["VALUE"] = new[] { EndKind == CalendarDateTimeKind.DateTime ? "DATE-TIME" : "DATE" } },
			});
			yield return Tuple.Create("DTSTAMP", new CalendarValue(TimeStamp.HasValue ? ToDateTimeString(TimeStamp.Value.ToUniversalTime()) : null));
			yield return Tuple.Create("DURATION", new CalendarValue(Duration.HasValue ? ToTimeSpanString(Duration.Value) : null));
			yield return Tuple.Create("SUMMARY", new CalendarValue(Summary));
			yield return Tuple.Create("DESCRIPTION", new CalendarValue(Description));
			yield return Tuple.Create("LOCATION", new CalendarValue(Location));
			yield return Tuple.Create("CATEGORIES", new CalendarValue(Categories));
			yield return Tuple.Create("TRANSPARENCY", new CalendarValue(Transparency?.ToString().ToUpper().Replace("ARENCY", "")));
		}
	}

	public enum CalendarTransparency
	{
		Opaque,
		Transparent,
	}
}