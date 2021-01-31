using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Core.Api
{
    public static class Shared
    {
        [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Initialized at startup by consumers to enable simpler extension methods")]
        public static ILoggerFactory LoggerFactory;
    }
}
