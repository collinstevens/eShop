using System;

namespace Core.Api.Utility
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
