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
    public partial class IncrementProductInCart : ApiController
    {
        public IncrementProductInCart(ILogger<IncrementProductInCart> logger, IStringLocalizer<Program> localizer, IMapper mapper, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Logger = logger;
            Localizer = localizer;
            Mapper = mapper;
            Context = context;
        }

        public ILogger Logger { get; }

        public IStringLocalizer<Program> Localizer { get; }

        public IMapper Mapper { get; }

        public CartContext Context { get; }

        [HttpPost("/api/cart/item")]
        public async Task<IActionResult> Handle(Request request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                return BadRequest();

            Logger.LogTrace("Received request to increment product \"{ProductId}\" in cart \"{CartId}\" by \"{Increment}\".", request.ProductId, request.CartId, request.Increment);

            CartEntity cart = await Context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                Logger.LogTrace("Cart \"{CartId}\" was not found.", request.CartId);
                string message = Localizer.GetStringSafe("CartNotFound", request.CartId);
                return NotFoundProblem(message);
            }

            ItemEntity item = await Context.Items.AsTracking().SingleOrDefaultAsync(c => c.CartId == request.CartId && c.ProductId == request.ProductId, cancellationToken);

            if (item is null)
            {
                Logger.LogTrace("Item for cart \"{CartId}\" and product \"{ProductId}\" was not found, creating new item with quantity \"{Quantity}\".",
                    request.CartId, request.ProductId, request.Increment);

                item = Mapper.Map<ItemEntity>(request);

                Context.Items.Add(item);
            }
            else
            {
                int existingQuantity = item.Quantity;
                item.Quantity += request.Increment;

                Logger.LogTrace("Updating item for cart \"{CartId}\" and product \"{ProductId}\" quantity to \"{NewQuantity}\" from \"{ExistingQuantity}\".",
                    request.CartId, request.ProductId, item.Quantity, existingQuantity);
            }

            await Context.SaveChangesAsync(cancellationToken);

            var response = Mapper.Map<Response>(item);

            return Ok(response);
        }

        public sealed new class Request
        {
            public Guid CartId { get; set; }

            public Guid ProductId { get; set; }

            public int Increment { get; set; }
        }

        public sealed class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(x => x.CartId).NotEmpty();
                RuleFor(x => x.ProductId).NotEmpty();
                RuleFor(x => x.Increment).GreaterThan(0);
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
                CreateMap<Request, ItemEntity>(MemberList.Source)
                    .ForMember(d => d.Quantity, opt => opt.MapFrom(s => s.Increment));
                CreateMap<ItemEntity, Response>(MemberList.Destination)
                    .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.ProductId));
            }
        }
    }
}
