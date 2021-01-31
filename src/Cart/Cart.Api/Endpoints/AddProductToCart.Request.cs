using FluentValidation;
using System;

namespace Cart.Api.Endpoints
{
    public partial class AddProductToCart
    {
        public new sealed class Request
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
    }
}
