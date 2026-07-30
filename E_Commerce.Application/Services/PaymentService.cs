using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Baskets;
using E_Commerce.Application.Specifications;
using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Orders;
using E_Commerce.Domain.Entities.Products;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IMapper _mapper;
        private readonly PaymentGatewaySettings _paymentGatewaySettings;

        public PaymentService(IBasketRepository basketRepository
            , IUnitOfWork unitOfWork 
            , IPaymentGateway paymentGateway
            , IOptions<PaymentGatewaySettings> options
            ,IMapper mapper
            )
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
           _paymentGateway = paymentGateway;
            _mapper = mapper;
            _paymentGatewaySettings = options.Value;
        }

        public async Task<Result<BasketDto>> CreateOrUpdatePaymentIntentAsync(string basketId, CancellationToken ct = default)
        {
            #region 1. Get Basket [Validate]
            var basket = await _basketRepository.GetBasketAsync(basketId, ct);

            if (basket == null)
                return Error.NotFound("Basket Is Not Found", $"Basket With Id {basketId} Is Not Found");

            if (basket.Items.Count == 0)
                return Error.Validation("Basket Is Empty");
            #endregion



            #region 2. Get Delivery Method -> Cost
            
            if (!basket.DeliveryMethodId.HasValue)
                return Error.Validation("Delivery Method Id Is Required");

            var deliveryMethod = await _unitOfWork.GetRepository<DeliveryMethod, int>().GetByIdAsync(basket.DeliveryMethodId.Value , ct);
            if (deliveryMethod == null)
                return Error.NotFound("Delivery Method Not Found");

            basket.ShippingPrice = deliveryMethod.Cost;

            #endregion

            #region 3. Product Prices

            var productsIds = basket.Items.Select(x => x.Id).ToHashSet();
            var products = (await _unitOfWork.GetRepository<Product, int>()
                .GetAllAsync(new ProductWithIdSpecifications(productsIds), ct)).ToDictionary(x => x.Id);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                    return Error.NotFound("Product Not Found");

                item.Price = product.Price;
            }
            #endregion


            // 4. Total Amount

            var subTotal = basket.Items.Sum(i => i.Price * i.Quantity);
            var amount = (long)((subTotal + deliveryMethod.Cost) * 100m);


            // 5.1 PaymentIntentId Empty => Create - Put PaymentIntentId + ClientSecret In Basket
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                // Create
                var result = await _paymentGateway.CreatePaymentIntentAsync(amount, _paymentGatewaySettings.DefaultCurrency, ct);
                basket.PaymentIntentId = result.PaymentIntentId;
                basket.ClientSecret = result.ClientSecret;
            }
            else
            {
                // 5.2 PaymentIntentId Not Empty -> Update PaymentIntent
                // Update
                await _paymentGateway.UpdatePaymentIntentAsync(amount, basket.PaymentIntentId, ct);

            }

            await _basketRepository.CreateOrUpdateBasketAsync(basket, ct: ct);
            // Return BasketDto Updated

            return _mapper.Map<BasketDto>(basket);
        }

        public async Task PaymentSucceeded(string paymentIntentId)
        {
            var order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(new PaymentIntentSpecefications(paymentIntentId));

            if (order == null)
                return;

            order.Status = OrderStatus.PaymentReceived;

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task PaymentFailed(string paymentIntentId)
        {
            var order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(new PaymentIntentSpecefications(paymentIntentId));

            if (order == null)
                return;

            order.Status = OrderStatus.PaymentFailed;

            await _unitOfWork.SaveChangesAsync(); 
            //
        }

    }
}
