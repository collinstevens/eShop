using AutoMapper;
using Cart.Api.Data;
using Cart.Api.Data.Models;
using Core.Api.Endpoints;
using Core.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Api.Endpoints
{
    public partial class GetCartById : ApiController
    {
        public GetCartById(ILogger<GetCartById> logger, IStringLocalizer<GetCartById> localizer, IMapper mapper, CartContext context)
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
        private readonly IStringLocalizer<GetCartById> _localizer;
        private readonly IMapper _mapper;
        private readonly CartContext _context;

        [HttpGet("/api/cart/{id}")]
        public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Received request to get cart by id \"{CartId}\".", id);

            CartEntity cart = await _context.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart is null)
            {
                _logger.LogTrace("Cart \"{CartId}\" was not found.", id);
                string message = _localizer.GetStringSafe("CartNotFound", id);
                return NotFoundProblem(message);
            }

            ItemEntity[] items = await _context.Items.AsNoTracking().Where(i => i.CartId == cart.Id).ToArrayAsync(cancellationToken);

            var response = _mapper.Map<Response>(cart);
            response.Items = items;

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
