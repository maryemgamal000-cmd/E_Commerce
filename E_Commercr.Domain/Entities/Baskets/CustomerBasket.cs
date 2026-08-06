using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.Entities.Baskets
{
    public class CustomerBasket
    {
        public string Id { get; set; } =default!;  //Created from FrontEnd Side [GUID]

        public ICollection<BasketItem> Items { get; set; } = [];
    }
}
