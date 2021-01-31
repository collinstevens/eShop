using Cart.Api.Data.Models;
using System;

namespace Cart.Api.Endpoints
{
    public partial class GetCartById
    {
        public new sealed class Response
        {
            public Guid CartId { get; set; }

            public ItemEntity[] Items { get; set; }
        }
    }
}
