using ExamTwo.Data.Models;
using Microsoft.Extensions.Logging;

namespace ExamTwo.Data.Repositories
{
    public class InMemoryDatabase : IDatabase
    {
        private readonly List<Coffee> _coffees;
        private readonly List<Coin> _coins;
        private readonly ILogger<InMemoryDatabase> _logger;

        public InMemoryDatabase(ILogger<InMemoryDatabase> logger = null)
        {
            _logger = logger;

            _coffees = new List<Coffee>
            {
                new Coffee { Name = "Americano", Price = 950, Quantity = 10 },
                new Coffee { Name = "Cappuccino", Price = 1200, Quantity = 8 },
                new Coffee { Name = "Lates", Price = 1350, Quantity = 10 },
                new Coffee { Name = "Mocaccino", Price = 1500, Quantity = 15 }
            };

            _coins = new List<Coin>
            {
                new Coin { Denomination = 1000, Quantity = 0 },
                new Coin { Denomination = 500, Quantity = 20 },
                new Coin { Denomination = 100, Quantity = 30 },
                new Coin { Denomination = 50, Quantity = 50 },
                new Coin { Denomination = 25, Quantity = 25 }
            };

            _logger?.LogInformation("InMemoryDatabase initialized with {CoffeeCount} coffees and {CoinCount} coin types",
                _coffees.Count, _coins.Count);
        }

        public List<Coffee> GetAllCoffees() => _coffees;

        public Coffee? GetCoffeeByName(string name)
        {
            var coffee = _coffees.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            _logger?.LogDebug("GetCoffeeByName: {Name} -> {Found}",
                name, coffee != null ? "Found" : "Not Found");

            return coffee;
        }

        public void UpdateCoffee(Coffee coffee)
        {
            var existing = GetCoffeeByName(coffee.Name);
            if (existing != null)
            {
                _logger?.LogInformation("Updating coffee {Name} quantity: {Old} -> {New}",
                    coffee.Name, existing.Quantity, coffee.Quantity);

                existing.Quantity = coffee.Quantity;
            }
            else
            {
                _logger?.LogWarning("Attempted to update non-existent coffee: {Name}", coffee.Name);
            }
        }

        public List<Coin> GetAllCoins() => _coins;

        public Coin? GetCoinByDenomination(int denomination)
        {
            var coin = _coins.FirstOrDefault(c => c.Denomination == denomination);

            _logger?.LogDebug("GetCoinByDenomination: {Denom} -> Quantity: {Quantity}",
                denomination, coin?.Quantity ?? 0);

            return coin;
        }

        public void UpdateCoin(Coin updatedCoin)
        {
            var existing = GetCoinByDenomination(updatedCoin.Denomination);
            if (existing != null)
            {
                _logger?.LogInformation("Updating coin {Denom} quantity: {Old} -> {New}",
                    updatedCoin.Denomination, existing.Quantity, updatedCoin.Quantity);

                existing.Quantity = updatedCoin.Quantity;
            }
            else
            {
                _logger?.LogWarning("Attempted to update non-existent coin: {Denom}", updatedCoin.Denomination);
            }
        }
    }
}