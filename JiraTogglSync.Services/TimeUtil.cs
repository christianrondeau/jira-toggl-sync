using System;

namespace JiraTogglSync.Services;

public interface ITimeUtil
{
	TimeSpan RoundToClosest(TimeSpan input, TimeSpan precision);
	DateTimeOffset RoundDateTimeToCloses(DateTimeOffset date, TimeSpan precision);
}

public class TimeUtil : ITimeUtil
{
	public TimeSpan RoundToClosest(TimeSpan input, TimeSpan precision)
	{
		if (input < TimeSpan.Zero)
		{
			return -RoundToClosest(-input, precision);
		}

		return new TimeSpan((input.Ticks + precision.Ticks / 2) / precision.Ticks * precision.Ticks);
	}

	public DateTimeOffset RoundDateTimeToCloses(DateTimeOffset date, TimeSpan precision)
	{
		var ticks = (date.Ticks + precision.Ticks / 2 + 1) / precision.Ticks;

		return new DateTimeOffset(ticks * precision.Ticks, date.Offset);
	}
}