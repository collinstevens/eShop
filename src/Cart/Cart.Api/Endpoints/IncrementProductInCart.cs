using AutoMapper;
using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Extensions;
using Core.Api.Utility;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public partial class IncrementProductInCart : ApiController
    {
        public IncrementProductInCart(ILogger<IncrementProductInCart> logger, IDiagnosticContext diagnosticsContext, IStringLocalizer<Program> localizer, IMapper mapper, IClock clock, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (diagnosticsContext is null)
                throw new ArgumentNullException(nameof(diagnosticsContext));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (clock is null)
                throw new ArgumentNullException(nameof(clock));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Logger = logger;
            DiagnosticsContext = diagnosticsContext;
            Localizer = localizer;
            Mapper = mapper;
            Clock = clock;
            Context = context;
        }

        public ILogger<IncrementProductInCart> Logger { get; }

        public IDiagnosticContext DiagnosticsContext { get; }

        public IStringLocalizer<Program> Localizer { get; }

        public IMapper Mapper { get; }

        public IClock Clock { get; }

        public CartContext Context { get; }

        [HttpPost("/api/cart/item")]
        public async Task<IActionResult> Handle(Request request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                return BadRequest();

            DiagnosticsContext.Set("CartId", request.CartId);
            DiagnosticsContext.Set("ProductId", request.ProductId);
            DiagnosticsContext.Set("Increment", request.Increment);

            CartEntity cart = await Context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                Logger.LogTrace("Cart not found");
                string message = Localizer.GetStringSafe("CartNotFound", request.CartId);
                return NotFoundProblem(message);
            }

            ItemEntity item = await Context.Items.AsTracking().SingleOrDefaultAsync(c => c.CartId == request.CartId && c.ProductId == request.ProductId, cancellationToken);

            if (item is null)
            {
                item = Mapper.Map<ItemEntity>(request);
                item.CreatedOn = Clock.UtcNow;
                item.ModifiedOn = Clock.UtcNow;

                DiagnosticsContext.Set("NewQuantity", item.Quantity);
                DiagnosticsContext.Set("PreviousQuantity", 0);

                Context.Items.Add(item);
            }
            else
            {
                int existingQuantity = item.Quantity;
                item.Quantity += request.Increment;
                item.ModifiedOn = Clock.UtcNow;

                DiagnosticsContext.Set("NewQuantity", item.Quantity);
                DiagnosticsContext.Set("PreviousQuantity", existingQuantity);
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
                    .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Increment))
                    .ForMember(d => d.CreatedOn, o => o.Ignore());
                CreateMap<ItemEntity, Response>(MemberList.Destination)
                    .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId));
            }
        }
    }
}
