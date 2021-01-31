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
    public partial class AddProductToCart : ApiController
    {
        public AddProductToCart(ILogger<AddProductToCart> logger, IStringLocalizer<AddProductToCart> localizer, CartContext context)
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
        private readonly IStringLocalizer<AddProductToCart> _localizer;
        private readonly CartContext _context;

        [HttpPost("/cart/item")]
        public async Task<IActionResult> Handle(Request request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                return BadRequest();

            _logger.LogTrace("Received request to add product '{ProductId}' to cart for user '{UserId}'.", request.ProductId.ToString("N"), request.CartId.ToString("N"));

            CartEntity cart = await _context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                _logger.LogTrace("Cart '{CartId}' was not found.", request.CartId.ToString("N"));
                string message = _localizer.GetResourceValue("CartNotFound", request.CartId.ToString("N"));
                return NotFoundProblem(message);
            }

            ItemEntity item = await _context.Items.AsTracking().SingleOrDefaultAsync(c => c.CartId == request.CartId && c.ProductId == request.ProductId, cancellationToken);

            if (item is null)
            {
                _logger.LogTrace("Item for cart '{CartId}' and product '{ProductId}' was not found, creating new Item with quantity '{Quantity}'.", 
                    request.CartId.ToString("N"), request.ProductId.ToString("N"), request.Increment);

                item = new()
                {
                    CartId = request.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Increment
                };

                _context.Items.Add(item);
            }
            else
            {
                int existingQuantity = item.Quantity;
                item.Quantity += request.Increment;

                _logger.LogTrace("Updating Item for cart '{CartId}' and product '{ProductID}' quantity to '{NewQuantity}' from '{ExistingQuantity}'.", 
                    request.CartId.ToString("N"), request.ProductId.ToString("N"), item.Quantity, existingQuantity);
            }

            await _context.SaveChangesAsync(cancellationToken);

            Response response = new()
            {

            };

            return Ok();
        }
    }
}
