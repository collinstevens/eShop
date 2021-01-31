using System;

namespace Cart.Api.Endpoints
{
    public partial class AddProductToCart
    {
        public new sealed class Response
        {
            public Guid ItemId { get; set; }

            public int Quantity { get; set; }
        }
    }
}
