using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Linearstar.Core.Calendar
{
	public class CalendarValue
	{
		public IList<string> Value { get; set; }
		public IDictionary<string, IList<string>> Parameters { get; set; }

		public CalendarValue() =>
			Parameters = new Dictionary<string, IList<string>>();

		public CalendarValue(params string[] value)
			: this()
		{
			if (value != null && value.Length > 0 && value[0] != null)
				Value = value.ToList();
		}

		public CalendarValue(IEnumerable<string> value)
			: this() =>
			Value = value?.ToList();

		protected string GetParameter([CallerMemberName] string propertyName = null)
		{
			propertyName = propertyName.ToUpper();

			return Parameters.ContainsKey(propertyName) ? Parameters[propertyName].First() : null;
		}

		protected string SetParameter(string value, [CallerMemberName] string propertyName = null) =>
			(Parameters[propertyName.ToUpper()] = new List<string>
			{
				value,
			}).First();

		public static string EscapeValueString(string s) =>
			s.Replace("\\", "\\\\")
			 .Replace(";", "\\;")
			 .Replace(",", "\\,")
			 .Replace("\r\n", "\\n")
			 .Replace("\n", "\\n");

		public static string UnescapeValueString(string s) =>
			s.Replace("\\;", ";")
			 .Replace("\\,", ",")
			 .Replace("\\n", "\r\n")
			 .Replace("\\\\", "\\");

		public static CalendarValue Parse(IEnumerable<string> value, ILookup<string, string> parameters) =>
			new CalendarValue(value)
			{
				Parameters = parameters.ToDictionary(_ => _.Key, _ => (IList<string>)_.ToList()),
			};
	}
}