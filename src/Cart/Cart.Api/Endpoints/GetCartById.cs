using AutoMapper;
using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public partial class GetCartById : ApiController
    {
        public GetCartById(ILogger<GetCartById> logger, IDiagnosticContext diagnosticsContext, IStringLocalizer<Program> localizer, IMapper mapper, CartContext context)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (diagnosticsContext is null)
                throw new ArgumentNullException(nameof(diagnosticsContext));

            if (localizer is null)
                throw new ArgumentNullException(nameof(localizer));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Logger = logger;
            DiagnosticsContext = diagnosticsContext;
            Localizer = localizer;
            Mapper = mapper;
            Context = context;
        }

        public ILogger<GetCartById> Logger { get; }

        public IDiagnosticContext DiagnosticsContext { get; }

        public IStringLocalizer<Program> Localizer { get; }

        public IMapper Mapper { get; }

        public CartContext Context { get; }


        [HttpGet("/api/cart/{id}")]
        public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken = default)
        {
            DiagnosticsContext.Set("CartId", id);

            CartEntity cart = await Context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart is null)
            {
                Logger.LogTrace("Cart not found");
                string message = Localizer.GetStringSafe("CartNotFound", id);
                return NotFoundProblem(message);
            }

            ItemEntity[] items = await Context.Items.AsNoTracking().Where(i => i.CartId == cart.Id).ToArrayAsync(cancellationToken);

            var response = Mapper.Map<Response>(cart);
            response.Items = items;

            DiagnosticsContext.Set("CartItemCount", items.Length);

            return Ok(response);
        }

        public sealed new class Response
        {
            public Guid CartId { get; set; }

            public ItemEntity[] Items { get; set; }
        }

        public sealed class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<CartEntity, Response>(MemberList.Destination)
                    .ForMember(d => d.CartId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(d => d.Items, opt => opt.Ignore());
            }
        }
    }
}
