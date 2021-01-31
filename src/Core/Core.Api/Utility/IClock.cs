using System;

namespace Core.Api.Utility
{
    public interface IClock
    {
        DateTime UtcNow { get; }

        DateTime HostNow { get; }
    }
}
