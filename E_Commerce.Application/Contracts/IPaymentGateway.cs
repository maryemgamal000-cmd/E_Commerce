using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_Commerce.Application.Common;


namespace E_Commerce.Application.Contracts
{
    public interface IPaymentGateway
    {
        // Create PaymentIntent
        // amount + Currency => PaymentIntentId + ClientSecret
        Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken ct = default);


        // Update PaymentIntent
        // PaymentIntentId + Amount => PaymentIntent + ClientSecret
        Task<PaymentIntentResult> UpdatePaymentIntentAsync(decimal amount, string paymentIntentId, CancellationToken ct = default);
    }
}
