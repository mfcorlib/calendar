using System;
using System.Collections.Generic;
using System.Linq;

namespace Linearstar.Core.Calendar
{
	public class CalendarDocument : CalendarItem
	{
		public const string Kind = "VCALENDAR";
		public const string DefaultProductId = "-//Linearstar//CoreCalendar//EN";
		public const string DefaultVersion = "2.0";
		public const string GregorianCalendarScale = "GREGORIAN";

		public string ProductId { get; set; }
		public string Version { get; set; }
		public string CalendarScale { get; set; }
		public string Method { get; set; }

		public CalendarDocument()
			: base(Kind)
		{
			ProductId = DefaultProductId;
			Version = DefaultVersion;
		}

		protected override CalendarValue ParseValue(string key, IReadOnlyList<string> value, ILookup<string, string> parameters)
		{
			switch (key)
			{
				case "PRODID":
					ProductId = value.First();

					return null;
				case "VERSION":
					Version = value.First();

					return null;
				case "CALSCALE":
					CalendarScale = value.First();

					return null;
				case "METHOD":
					Method = value.First();

					return null;
				default:
					return base.ParseValue(key, value, parameters);
			}
		}

		protected override IEnumerable<Tuple<string, CalendarValue>> WriteValues()
		{
			yield return Tuple.Create("PRODID", new CalendarValue(ProductId));
			yield return Tuple.Create("VERSION", new CalendarValue(Version));
			yield return Tuple.Create("CALSCALE", new CalendarValue(CalendarScale));
			yield return Tuple.Create("METHOD", new CalendarValue(Method));
		}
	}
}
