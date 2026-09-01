using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oqtane.Modules;
using Stripe;
using Stripe.Checkout;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Manager
{
    public class StripeService : IStripeService, ITransientService
    {
        private readonly IConfiguration _configuration;

        // StripeConfiguration.ApiKey is a process-wide static - only needs setting once,
        // regardless of how many transient StripeService instances DI creates.
        private static bool _apiKeySet;
        private static readonly object _apiKeyLock = new();

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            EnsureApiKeySet();
        }

        private void EnsureApiKeySet()
        {
            if (_apiKeySet)
            {
                return;
            }

            lock (_apiKeyLock)
            {
                if (_apiKeySet)
                {
                    return;
                }

                StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
                _apiKeySet = true;
            }
        }

        public async Task<string> CreateFuturityCheckoutSessionAsync(int participationId, string successUrl, string cancelUrl)
        {
            var productId = _configuration["Stripe:FuturityProductId"];

            return await CreateCheckoutSessionAsync(productId, successUrl, cancelUrl, new Dictionary<string, string>
            {
                { "Purpose", "FuturityNomination" },
                { "ParticipationId", participationId.ToString() }
            });
        }

        public async Task<string> CreateMembershipCheckoutSessionAsync(int membershipId, string membershipType, string successUrl, string cancelUrl)
        {
            var productId = _configuration[$"Stripe:MembershipProductIds:{membershipType}"];
            if (string.IsNullOrEmpty(productId))
            {
                throw new InvalidOperationException(
                    $"No Stripe product configured for membership type '{membershipType}' (Stripe:MembershipProductIds:{membershipType}).");
            }

            return await CreateCheckoutSessionAsync(productId, successUrl, cancelUrl, new Dictionary<string, string>
            {
                { "Purpose", "MembershipPurchase" },
                { "MembershipId", membershipId.ToString() }
            });
        }

        private async Task<string> CreateCheckoutSessionAsync(string productId, string successUrl, string cancelUrl, Dictionary<string, string> metadata)
        {
            var priceId = await ResolveActivePriceIdAsync(productId);

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                // Restricting to plain card entry keeps Stripe from offering Link (its
                // cross-merchant saved-card/SMS-verification feature) - a one-off
                // payment doesn't need it, and its verification UI has been unreliable
                // enough in testing to just avoid entirely.
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = metadata
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(options);

            return session.Url;
        }

        // Prefers the product's default price (so a price change in the Dashboard just
        // works), but falls back to any active price on the product - a Product created
        // via the CLI without explicitly setting a default price otherwise has none,
        // even though a real Price exists and is perfectly usable.
        private async Task<string> ResolveActivePriceIdAsync(string productId)
        {
            var productService = new ProductService();
            var product = await productService.GetAsync(productId);

            if (!string.IsNullOrEmpty(product.DefaultPriceId))
            {
                return product.DefaultPriceId;
            }

            var priceService = new PriceService();
            var prices = await priceService.ListAsync(new PriceListOptions
            {
                Product = productId,
                Active = true,
                Limit = 1
            });

            var priceId = prices.Data.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(priceId))
            {
                throw new InvalidOperationException(
                    $"Stripe product {productId} has no active price. Add a price to it in the Stripe Dashboard.");
            }

            return priceId;
        }

        public Event ConstructWebhookEvent(string json, string stripeSignatureHeader)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            // The account's events are tagged with an older API version than this
            // Stripe.net version expects, which throws by default on a mismatch. Safe to
            // disable here - the fields this app actually reads off a Checkout Session
            // (Id, Metadata, AmountTotal, PaymentIntentId) have been stable across
            // versions for years.
            return EventUtility.ConstructEvent(json, stripeSignatureHeader, webhookSecret, throwOnApiVersionMismatch: false);
        }
    }
}
