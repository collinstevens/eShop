using Cart.Api.Data;
using Cart.Api.Data.Models;
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
        public CreateCart(ILogger<CreateCart> logger, IStringLocalizer<CreateCart> localizer, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _logger = logger;
            _localizer = localizer;
            _context = context;
        }

        private readonly ILogger _logger;
        private readonly IStringLocalizer<CreateCart> _localizer;
        private readonly CartContext _context;

        [HttpPost("/cart")]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Received request to create cart.");

            CartEntity cart = new();

            _context.Add(cart);

            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetCartById.Handle), nameof(GetCartById), new { cart.Id }, cart);
        }
    }
}
