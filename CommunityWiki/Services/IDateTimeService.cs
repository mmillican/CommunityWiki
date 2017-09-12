using System;

namespace CommunityWiki.Services
{
    public interface IDateTimeService
    {
        DateTime GetNowUtc();
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTime GetNowUtc() => DateTime.UtcNow;
    }
}
