using System;

namespace JiraTogglSync.Services;

public interface ITimeUtil
{
	TimeSpan RoundToClosest(TimeSpan input, TimeSpan precision);
	DateTime RoundDateTimeToCloses(DateTime date, TimeSpan precision);
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

	public DateTime RoundDateTimeToCloses(DateTime date, TimeSpan precision)
	{
		var ticks = (date.Ticks + precision.Ticks / 2 + 1) / precision.Ticks;

		return new DateTime(ticks * precision.Ticks, date.Kind);
	}
}