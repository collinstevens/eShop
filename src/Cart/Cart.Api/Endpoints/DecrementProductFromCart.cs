using AutoMapper;
using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public partial class DecrementProductFromCart : ApiController
    {
        public DecrementProductFromCart(ILogger<IncrementProductInCart> logger, IStringLocalizer<DecrementProductFromCart> localizer, IMapper mapper, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _context = context;
        }

        private readonly ILogger _logger;
        private readonly IStringLocalizer<DecrementProductFromCart> _localizer;
        private readonly IMapper _mapper;
        private readonly CartContext _context;

        [HttpDelete("/api/cart/item")]
        public async Task<IActionResult> Handle(Request request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                return BadRequest();

            _logger.LogTrace("Received request to decrement product '{ProductId}' from cart '{CartId}' by '{Decrement}'.", request.ProductId, request.CartId, request.Decrement);

            CartEntity cart = await _context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                _logger.LogTrace("Cart '{CartId}' was not found.", request.CartId);
                string message = _localizer.GetStringSafe("CartNotFound", request.CartId);
                return NotFoundProblem(message);
            }

            ItemEntity item = await _context.Items.AsTracking().SingleOrDefaultAsync(c => c.CartId == request.CartId && c.ProductId == request.ProductId, cancellationToken);

            if (item is null)
            {
                _logger.LogTrace("Product '{ProductId}' was not found in cart '{CartId}'.", request.ProductId, request.CartId);
                string message = _localizer.GetStringSafe("ProductNotFoundInCart", request.ProductId, request.CartId);
                return NotFoundProblem(message);
            }

            int existingQuantity = item.Quantity;
            item.Quantity = Math.Max(0, existingQuantity - request.Decrement);

            _logger.LogTrace("Updating item for cart '{CartId}' and product '{ProductID}' quantity to '{NewQuantity}' from '{ExistingQuantity}'.", 
                request.CartId, request.ProductId, item.Quantity, existingQuantity);

            await _context.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<Response>(item);

            return Ok(response);
        }

        public sealed new class Request
        {
            public Guid CartId { get; set; }

            public Guid ProductId { get; set; }

            public int Decrement { get; set; }
        }

        public sealed class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(x => x.CartId).NotEmpty();
                RuleFor(x => x.ProductId).NotEmpty();
                RuleFor(x => x.Decrement).GreaterThan(0);
            }
        }

        public sealed new class Response
        {
            public Guid ProductId { get; set; }

            public int Quantity { get; set; }
        }

        public sealed class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<ItemEntity, Response>(MemberList.Destination)
                    .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.ProductId));
            }
        }
    }
}
