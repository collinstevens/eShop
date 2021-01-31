using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Core.Api.Extensions
{
    public static class StringLocalizerExtensions
    {
        private readonly static Lazy<ILogger> _loggerLazy = new Lazy<ILogger>(Shared.LoggerFactory.CreateLogger(typeof(StringLocalizerExtensions)));

        private readonly static ILogger _logger = _loggerLazy.Value;

        public static string GetStringSafe(this IStringLocalizer localizer, string name, params object[] arguments)
        {
            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));

            LocalizedString resource = localizer.GetString(name, arguments);

            if (resource.ResourceNotFound)
            {
                string culture = Thread.CurrentThread.CurrentCulture.Name;
                _logger.LogWarning("Globalized resource '{ResourceName}' not found for '{Culture}'.", resource.Name, culture);
                return "Globalized resource not found.";
            }

            return resource.Value;
        }
    }
}
