using System;

namespace Core.Api.Utility
{
    public class Clock : IClock
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
