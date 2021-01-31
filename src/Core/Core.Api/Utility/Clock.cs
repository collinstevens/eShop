using System;

namespace Core.Api.Utility
{
    public class Clock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime HostNow => DateTime.Now;
    }
}
