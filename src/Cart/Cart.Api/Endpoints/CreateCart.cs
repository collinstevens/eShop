using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public class CreateCart : ApiController
    {
        public CreateCart(ILogger<CreateCart> logger, IDiagnosticContext diagnosticsContext, IStringLocalizer<Program> localizer, IClock clock, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (diagnosticsContext is null)
                throw new ArgumentNullException(nameof(diagnosticsContext));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (clock is null)
                throw new ArgumentNullException(nameof(clock));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Logger = logger;
            DiagnosticsContext = diagnosticsContext;
            Localizer = localizer;
            Clock = clock;
            Context = context;
        }

        public ILogger<CreateCart> Logger { get; }

        public IDiagnosticContext DiagnosticsContext { get; }

        public IStringLocalizer<Program> Localizer { get; }

        public IClock Clock { get; }

        public CartContext Context { get; }

        [HttpPost("/api/cart")]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken = default)
        {
            CartEntity cart = new()
            {
                CreatedOn = Clock.UtcNow
            };

            Context.Add(cart);

            await Context.SaveChangesAsync(cancellationToken);

            DiagnosticsContext.Set("CartId", cart.Id);

            return CreatedAtAction(nameof(GetCartById.Handle), nameof(GetCartById), new { cart.Id }, cart);
        }
    }
}
