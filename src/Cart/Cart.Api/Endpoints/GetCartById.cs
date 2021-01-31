using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public partial class GetCartById : ApiController
    {
        public GetCartById(ILogger<GetCartById> logger, IStringLocalizer<GetCartById> localizer, CartContext context)
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
        private readonly IStringLocalizer<GetCartById> _localizer;
        private readonly CartContext _context;

        [HttpGet("/cart/{id}")]
        public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Received request to get cart by id '{CartId}'.", id.ToString("N"));

            CartEntity cart = await _context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart is null)
            {
                _logger.LogTrace("Cart '{CartId}' was not found.", id.ToString("N"));
                string message = _localizer.GetResourceValue("CartNotFound", id.ToString("N"));
                return NotFoundProblem(message);
            }

            Response response = new()
            {
                CartId = cart.Id,
                Items = Array.Empty<ItemEntity>()
            };

            return Ok(response);
        }
    }
}
