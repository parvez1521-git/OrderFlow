/* Checkout contracts: the client chooses payment behavior only for learning demos. */
using System.ComponentModel.DataAnnotations;
using OrderFlow.Api.Features.Cart;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Features.Payment;

namespace OrderFlow.Api.Features.Checkout;

public sealed record CheckoutRequest([Required] string PaymentMode, [Required] string IdempotencyKey);
public sealed record CheckoutResponse(OrderResponse Order, PaymentResponse Payment, CartResponse CartSnapshot);
