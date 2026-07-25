using E_Commercr.Domain.Entities.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commercr.Domain.Contracts
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetBasketAsync(string basketId, CancellationToken ct = default);

        Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket, TimeSpan? timeToLive = default, CancellationToken ct = default);

        Task<bool> DeleteBasketAsync(string basketId, CancellationToken ct = default);
    }
}
