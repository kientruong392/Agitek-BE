using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Carts;
using AgitekBackend.Services.Interfaces;
using StackExchange.Redis;

namespace AgitekBackend.Services
{
    public class CartService : ICartService
    {
        private static readonly TimeSpan CartLifetime = TimeSpan.FromDays(7);
        private readonly IDatabase _database;

        public CartService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task AddToCartAsync(string cartKey, string productId, int quantity)
        {
            ValidateCartArguments(cartKey, productId);
            if (quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero");

            await _database.HashIncrementAsync(cartKey, productId, quantity);
            await _database.KeyExpireAsync(cartKey, CartLifetime);
        }

        public async Task<IEnumerable<CartItemDTO>> GetCartAsync(string cartKey)
        {
            ValidateCartKey(cartKey);
            var entries = await _database.HashGetAllAsync(cartKey);
            await _database.KeyExpireAsync(cartKey, CartLifetime);
            if (entries.Length == 0)
                return Array.Empty<CartItemDTO>();

            return entries
                .Select(entry => new CartItemDTO
                {
                    ProductId = int.Parse(entry.Name!),
                    Quantity = (int)entry.Value
                })
                .ToList();
        }

        public async Task RemoveFromCartAsync(string cartKey, string productId)
        {
            ValidateCartArguments(cartKey, productId);
            await _database.HashDeleteAsync(cartKey, productId);
        }

        public async Task MergeCartAsync(string guestCartKey, string userCartKey)
        {
            ValidateCartKey(guestCartKey);
            ValidateCartKey(userCartKey);
            if (guestCartKey == userCartKey)
                return;

            const string mergeScript = @"
                local entries = redis.call('HGETALL', KEYS[1])
                for i = 1, #entries, 2 do
                    redis.call('HINCRBY', KEYS[2], entries[i], tonumber(entries[i + 1]))
                end
                redis.call('DEL', KEYS[1])
                if redis.call('EXISTS', KEYS[2]) == 1 then
                    redis.call('EXPIRE', KEYS[2], ARGV[1])
                end
                return #entries / 2";

            await _database.ScriptEvaluateAsync(
                mergeScript,
                new RedisKey[] { guestCartKey, userCartKey },
                new RedisValue[] { (int)CartLifetime.TotalSeconds });
        }

        private static void ValidateCartArguments(string cartKey, string productId)
        {
            ValidateCartKey(cartKey);
            if (!int.TryParse(productId, out var parsedProductId) || parsedProductId <= 0)
                throw new BadRequestException("Invalid product ID");
        }

        private static void ValidateCartKey(string cartKey)
        {
            if (string.IsNullOrWhiteSpace(cartKey))
                throw new BadRequestException("Invalid cart key");
        }
    }
}
