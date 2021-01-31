using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public class CreateCart : ApiController
    {
        public CreateCart(ILogger<CreateCart> logger, IStringLocalizer<Program> localizer, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Logger = logger;
            Localizer = localizer;
            Context = context;
        }

        public ILogger Logger { get; }

        public IStringLocalizer<Program> Localizer { get; }

        public CartContext Context { get; }

        [HttpPost("/api/cart")]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Received request to create cart.");

            CartEntity cart = new();

            Context.Add(cart);

            await Context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetCartById.Handle), nameof(GetCartById), new { cart.Id }, cart);
        }
    }
}
